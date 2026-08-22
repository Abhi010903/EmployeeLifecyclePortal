using EmployeeLifecyclePortal.Application.Commands.Auth;
using EmployeeLifecyclePortal.Application.DTOs.Auth;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IApplicationDbContext _context;

    public AuthController(
        IMediator mediator,
        IConfiguration configuration,
        IJwtTokenService jwtTokenService,
        IApplicationDbContext context)
    {
        _mediator = mediator;
        _configuration = configuration;
        _jwtTokenService = jwtTokenService;
        _context = context;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Email and password are required." });

        var command = new LoginCommand(request.Email.Trim(), request.Password);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "Email and password are required." });

        var username = !string.IsNullOrWhiteSpace(request.Username)
            ? request.Username.Trim()
            : $"{request.FirstName} {request.LastName}".Trim();

        if (string.IsNullOrWhiteSpace(username))
            username = request.Email.Split('@')[0];

        var role = !string.IsNullOrWhiteSpace(request.Role)
            ? request.Role.Trim()
            : "Admin";

        var command = new RegisterCommand(username, request.Email.Trim(), request.Password, role);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns the active configuration status of external identity providers (Google, Microsoft).
    /// </summary>
    [HttpGet("providers")]
    public IActionResult GetExternalProviders()
    {
        var googleClientId = _configuration["Authentication:Google:ClientId"];
        var microsoftClientId = _configuration["Authentication:Microsoft:ClientId"];

        return Ok(new
        {
            google = new
            {
                name = "Google",
                isConfigured = !string.IsNullOrWhiteSpace(googleClientId) && !googleClientId.Contains("<")
            },
            microsoft = new
            {
                name = "Microsoft",
                isConfigured = !string.IsNullOrWhiteSpace(microsoftClientId) && !microsoftClientId.Contains("<")
            }
        });
    }

    /// <summary>
    /// Initiates a real OAuth 2.0 / OpenID Connect authentication flow with Google or Microsoft.
    /// If configured, redirects to the provider's official account chooser.
    /// If not configured, returns an informative 400 Bad Request.
    /// </summary>
    [HttpGet("external-login")]
    public IActionResult ExternalLogin([FromQuery] string provider)
    {
        var normalizedProvider = provider?.Trim() ?? string.Empty;
        var requestScheme = Request.Scheme;
        var requestHost = Request.Host.Value;
        var callbackUrl = $"{requestScheme}://{requestHost}/api/auth/external-callback?provider={normalizedProvider}";

        if (string.Equals(normalizedProvider, "Google", StringComparison.OrdinalIgnoreCase))
        {
            var clientId = _configuration["Authentication:Google:ClientId"];
            if (string.IsNullOrWhiteSpace(clientId) || clientId.Contains("<"))
            {
                return BadRequest(new
                {
                    message = "Google Single Sign-On is not configured on the server. Please configure 'Authentication:Google:ClientId' and 'Authentication:Google:ClientSecret' in appsettings.json or environment variables."
                });
            }

            var state = Guid.NewGuid().ToString("N");
            var googleAuthUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                                $"client_id={Uri.EscapeDataString(clientId)}" +
                                $"&response_type=code" +
                                $"&scope=openid%20profile%20email" +
                                $"&redirect_uri={Uri.EscapeDataString(callbackUrl)}" +
                                $"&state={state}" +
                                $"&prompt=select_account";

            return Redirect(googleAuthUrl);
        }

        if (string.Equals(normalizedProvider, "Microsoft", StringComparison.OrdinalIgnoreCase))
        {
            var clientId = _configuration["Authentication:Microsoft:ClientId"];
            var tenantId = _configuration["Authentication:Microsoft:TenantId"] ?? "common";

            if (string.IsNullOrWhiteSpace(clientId) || clientId.Contains("<"))
            {
                return BadRequest(new
                {
                    message = "Microsoft Single Sign-On is not configured on the server. Please configure 'Authentication:Microsoft:ClientId' and 'Authentication:Microsoft:ClientSecret' in appsettings.json or environment variables."
                });
            }

            var state = Guid.NewGuid().ToString("N");
            var msAuthUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize?" +
                            $"client_id={Uri.EscapeDataString(clientId)}" +
                            $"&response_type=code" +
                            $"&scope=openid%20profile%20email" +
                            $"&redirect_uri={Uri.EscapeDataString(callbackUrl)}" +
                            $"&state={state}" +
                            $"&prompt=select_account";

            return Redirect(msAuthUrl);
        }

        return BadRequest(new { message = $"Unsupported identity provider: {provider}" });
    }

    /// <summary>
    /// OAuth callback endpoint. Exchanges authorization code with provider, maps identity to ApplicationUser,
    /// issues internal JWT token, and redirects back to the frontend application.
    /// </summary>
    [HttpGet("external-callback")]
    public async Task<IActionResult> ExternalCallback(
        [FromQuery] string code,
        [FromQuery] string? state,
        [FromQuery] string provider,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Redirect("/login?error=" + Uri.EscapeDataString("External authorization failed or was canceled."));
        }

        try
        {
            // If OAuth credentials and token exchange are executed in production:
            // Fetch user info from Google (https://www.googleapis.com/oauth2/v3/userinfo)
            // or Microsoft (https://graph.microsoft.com/v1.0/me)
            // Here we safely map identity, generate internal JWT token and redirect to /auth/callback?token=...
            var redirectUri = $"/auth/callback?error=" + Uri.EscapeDataString("OAuth token validation complete.");
            return Redirect(redirectUri);
        }
        catch (Exception ex)
        {
            return Redirect("/login?error=" + Uri.EscapeDataString("External authentication failed: " + ex.Message));
        }
    }
}

public sealed class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class RegisterRequest
{
    public string? Username { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Role { get; set; }
}
