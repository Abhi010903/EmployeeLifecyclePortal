namespace EmployeeLifecyclePortal.Application.DTOs.Settings;

public class CompanyProfileDto
{
    public Guid Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyCode { get; set; }
    public string? IndustryType { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public string? LogoPath { get; set; }
    public string? RegistrationNumber { get; set; }
    public DateTime? FoundedDate { get; set; }
}

public class UserSettingsDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Theme { get; set; }
    public string? Language { get; set; }
    public string? TimeZone { get; set; }
    public bool EmailNotifications { get; set; }
    public bool SmsNotifications { get; set; }
    public bool PushNotifications { get; set; }
    public bool TwoFactorEnabled { get; set; }
}

public class HolidayCalendarDto
{
    public Guid Id { get; set; }
    public string HolidayName { get; set; } = string.Empty;
    public DateTime HolidayDate { get; set; }
    public string? Description { get; set; }
    public bool IsOptional { get; set; }
    public int Year { get; set; }
}

public class ShiftDto
{
    public Guid Id { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int WorkingHours { get; set; }
    public bool IsActive { get; set; }
}

public class WorkingHoursDto
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public Guid ShiftId { get; set; }
    public string? ShiftName { get; set; }
    public DateTime EffectiveFromUtc { get; set; }
    public DateTime? EffectiveToUtc { get; set; }
    public bool IsActive { get; set; }
}

public class EmailConfigurationDto
{
    public Guid Id { get; set; }
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string SenderEmail { get; set; } = string.Empty;
    public string? SenderName { get; set; }
    public bool EnableSsl { get; set; }
    public string? Username { get; set; }
}
