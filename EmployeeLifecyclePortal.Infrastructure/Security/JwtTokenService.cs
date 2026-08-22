using EmployeeLifecyclePortal.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmployeeLifecyclePortal.Infrastructure.Security;

public sealed class JwtTokenService
    : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(
        Guid userId,
        string email,
        string role)
    {
        var secret = _configuration["Jwt:Key"] 
            ?? _configuration["Jwt:Secret"] 
            ?? _configuration["JWT_SECRET"];

        if (string.IsNullOrWhiteSpace(secret) || secret == "${JWT_SECRET}" || secret.StartsWith("<"))
        {
            var envSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
                ?? Environment.GetEnvironmentVariable("Jwt__Key")
                ?? Environment.GetEnvironmentVariable("Jwt__Secret");

            if (!string.IsNullOrWhiteSpace(envSecret))
            {
                secret = envSecret;
            }
            else
            {
                throw new InvalidOperationException("JWT signing key is not configured. Please set the 'JWT_SECRET' environment variable or configure 'Jwt:Key' / 'Jwt:Secret'.");
            }
        }

        var issuer = _configuration["Jwt:Issuer"] ?? "EmployeeLifecyclePortal";
        var audience = _configuration["Jwt:Audience"] ?? "EmployeeLifecyclePortalUsers";

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secret));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                userId.ToString()),

            new Claim(
                ClaimTypes.Email,
                email),

            new Claim(
                ClaimTypes.Role,
                role)
        };

        var token =
            new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}