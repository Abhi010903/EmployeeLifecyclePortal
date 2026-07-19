using EmployeeLifecyclePortal.Application.Authorization;
using EmployeeLifecyclePortal.Application.DTOs.Settings;
using EmployeeLifecyclePortal.Domain.Entities;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Employee)]
public sealed class SettingsController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IMediator _mediator;

    public SettingsController(IApplicationDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    // Company Profile
    [HttpGet("company")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCompanyProfile(CancellationToken cancellationToken)
    {
        var profile = await _context.CompanyProfiles.FirstOrDefaultAsync(cancellationToken);
        if (profile == null)
            return NotFound();

        return Ok(new CompanyProfileDto
        {
            Id = profile.Id,
            CompanyName = profile.CompanyName,
            CompanyCode = profile.CompanyCode,
            IndustryType = profile.IndustryType,
            Address = profile.Address,
            City = profile.City,
            State = profile.State,
            Country = profile.Country,
            PostalCode = profile.PostalCode,
            PhoneNumber = profile.PhoneNumber,
            Email = profile.Email,
            Website = profile.Website,
            LogoPath = profile.LogoPath,
            RegistrationNumber = profile.RegistrationNumber,
            FoundedDate = profile.FoundedDate
        });
    }

    [HttpPut("company")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> UpdateCompanyProfile([FromBody] CompanyProfileDto dto, CancellationToken cancellationToken)
    {
        var profile = await _context.CompanyProfiles.FirstOrDefaultAsync(cancellationToken);
        if (profile == null)
        {
            profile = new CompanyProfile(dto.CompanyName);
            _context.CompanyProfiles.Add(profile);
        }

        profile.UpdateProfile(dto.CompanyName, dto.CompanyCode, dto.IndustryType, dto.Address,
            dto.City, dto.State, dto.Country, dto.PostalCode, dto.PhoneNumber, dto.Email,
            dto.Website, dto.RegistrationNumber, dto.FoundedDate);

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(dto);
    }

    // User Settings
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetUserSettings(Guid userId, CancellationToken cancellationToken)
    {
        var settings = await _context.UserSettings.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        if (settings == null)
        {
            settings = new UserSettings(userId);
            _context.UserSettings.Add(settings);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Ok(new UserSettingsDto
        {
            Id = settings.Id,
            UserId = settings.UserId,
            Theme = settings.Theme,
            Language = settings.Language,
            TimeZone = settings.TimeZone,
            EmailNotifications = settings.EmailNotifications,
            SmsNotifications = settings.SmsNotifications,
            PushNotifications = settings.PushNotifications,
            TwoFactorEnabled = settings.TwoFactorEnabled
        });
    }

    [HttpPut("user/{userId:guid}")]
    public async Task<IActionResult> UpdateUserSettings(Guid userId, [FromBody] UserSettingsDto dto, CancellationToken cancellationToken)
    {
        var settings = await _context.UserSettings.FirstOrDefaultAsync(u => u.UserId == userId, cancellationToken);
        if (settings == null)
        {
            settings = new UserSettings(userId);
            _context.UserSettings.Add(settings);
        }

        settings.UpdatePreferences(dto.Theme, dto.Language, dto.TimeZone);
        settings.UpdateNotifications(dto.EmailNotifications, dto.SmsNotifications, dto.PushNotifications);

        await _context.SaveChangesAsync(cancellationToken);
        return Ok(dto);
    }

    // Holidays
    [HttpGet("holidays/{year}")]
    public async Task<IActionResult> GetHolidays(int year, CancellationToken cancellationToken)
    {
        var holidays = await _context.HolidayCalendars
            .Where(h => h.Year == year)
            .OrderBy(h => h.HolidayDate)
            .ToListAsync(cancellationToken);

        return Ok(holidays.Select(h => new HolidayCalendarDto
        {
            Id = h.Id,
            HolidayName = h.HolidayName,
            HolidayDate = h.HolidayDate,
            Description = h.Description,
            IsOptional = h.IsOptional,
            Year = h.Year
        }));
    }

    [HttpPost("holidays")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> CreateHoliday([FromBody] HolidayCalendarDto dto, CancellationToken cancellationToken)
    {
        var holiday = new HolidayCalendar(dto.HolidayName, dto.HolidayDate, dto.Year);
        _context.HolidayCalendars.Add(holiday);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetHolidays), new { year = dto.Year }, dto);
    }

    // Shifts
    [HttpGet("shifts")]
    public async Task<IActionResult> GetShifts(CancellationToken cancellationToken)
    {
        var shifts = await _context.Shifts
            .Where(s => s.IsActive)
            .ToListAsync(cancellationToken);

        return Ok(shifts.Select(s => new ShiftDto
        {
            Id = s.Id,
            ShiftName = s.ShiftName,
            StartTime = s.StartTime.ToString(@"hh\:mm"),
            EndTime = s.EndTime.ToString(@"hh\:mm"),
            WorkingHours = s.WorkingHours,
            IsActive = s.IsActive
        }));
    }

    [HttpPost("shifts")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> CreateShift([FromBody] ShiftDto dto, CancellationToken cancellationToken)
    {
        if (!TimeSpan.TryParse(dto.StartTime, out var start) || !TimeSpan.TryParse(dto.EndTime, out var end))
            return BadRequest("Invalid time format");

        var shift = new Shift(dto.ShiftName, start, end, dto.WorkingHours);
        _context.Shifts.Add(shift);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetShifts), new { id = shift.Id }, dto);
    }

    // Working Hours
    [HttpGet("working-hours/employee/{employeeId:guid}")]
    public async Task<IActionResult> GetEmployeeWorkingHours(Guid employeeId, CancellationToken cancellationToken)
    {
        var workingHours = await _context.WorkingHours
            .Where(w => w.EmployeeId == employeeId && w.IsActive)
            .Include(w => w.Employee)
            .Include(w => w.Shift)
            .FirstOrDefaultAsync(cancellationToken);

        if (workingHours == null)
            return NotFound();

        return Ok(new WorkingHoursDto
        {
            Id = workingHours.Id,
            EmployeeId = workingHours.EmployeeId,
            EmployeeName = workingHours.Employee != null ? $"{workingHours.Employee.FirstName} {workingHours.Employee.LastName}" : "Unknown",
            ShiftId = workingHours.ShiftId,
            ShiftName = workingHours.Shift?.ShiftName,
            EffectiveFromUtc = workingHours.EffectiveFromUtc,
            EffectiveToUtc = workingHours.EffectiveToUtc,
            IsActive = workingHours.IsActive
        });
    }

    [HttpPost("working-hours")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> AssignWorkingHours([FromBody] WorkingHoursDto dto, CancellationToken cancellationToken)
    {
        var workingHours = new WorkingHours(dto.EmployeeId, dto.ShiftId);
        _context.WorkingHours.Add(workingHours);
        await _context.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetEmployeeWorkingHours), new { employeeId = dto.EmployeeId }, dto);
    }

    // Email Config
    [HttpGet("email-config")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> GetEmailConfig(CancellationToken cancellationToken)
    {
        var config = await _context.EmailConfigurations.FirstOrDefaultAsync(cancellationToken);
        if (config == null)
            return NotFound();

        return Ok(new EmailConfigurationDto
        {
            Id = config.Id,
            SmtpServer = config.SmtpServer,
            SmtpPort = config.SmtpPort,
            SenderEmail = config.SenderEmail,
            SenderName = config.SenderName,
            EnableSsl = config.EnableSsl,
            Username = config.Username
        });
    }

    [HttpPut("email-config")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> UpdateEmailConfig([FromBody] EmailConfigurationDto dto, CancellationToken cancellationToken)
    {
        var config = await _context.EmailConfigurations.FirstOrDefaultAsync(cancellationToken);
        if (config == null)
        {
            config = new EmailConfiguration(dto.SmtpServer, dto.SmtpPort, dto.SenderEmail);
            _context.EmailConfigurations.Add(config);
        }

        config.UpdateConfiguration(dto.SmtpServer, dto.SmtpPort, dto.SenderEmail, dto.EnableSsl, dto.Username);
        await _context.SaveChangesAsync(cancellationToken);

        return Ok(dto);
    }
}
