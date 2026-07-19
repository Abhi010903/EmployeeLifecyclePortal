using EmployeeLifecyclePortal.Domain.Common;

namespace EmployeeLifecyclePortal.Domain.Entities;

/// <summary>Sprint 31: Settings - Company, User, Organization, Shift, Holiday</summary>
public class CompanyProfile : AuditableEntity
{
    public string CompanyName { get; private set; } = string.Empty;
    public string? CompanyCode { get; private set; }
    public string? IndustryType { get; private set; }
    public string? Address { get; private set; }
    public string? City { get; private set; }
    public string? State { get; private set; }
    public string? Country { get; private set; }
    public string? PostalCode { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Email { get; private set; }
    public string? Website { get; private set; }
    public string? LogoPath { get; private set; }
    public string? RegistrationNumber { get; private set; }
    public DateTime? FoundedDate { get; private set; }

    private CompanyProfile() { }

    public CompanyProfile(string companyName)
    {
        CompanyName = companyName;
    }

    public void UpdateProfile(string name, string? code, string? industry, string? address,
        string? city, string? state, string? country, string? postal, string? phone, 
        string? email, string? website, string? registration, DateTime? founded)
    {
        CompanyName = name;
        CompanyCode = code;
        IndustryType = industry;
        Address = address;
        City = city;
        State = state;
        Country = country;
        PostalCode = postal;
        PhoneNumber = phone;
        Email = email;
        Website = website;
        RegistrationNumber = registration;
        FoundedDate = founded;
    }
}

public class UserSettings : AuditableEntity
{
    public Guid UserId { get; private set; }
    public string? Theme { get; private set; } = "light";
    public string? Language { get; private set; } = "en";
    public string? TimeZone { get; private set; } = "UTC";
    public bool EmailNotifications { get; private set; } = true;
    public bool SmsNotifications { get; private set; } = false;
    public bool PushNotifications { get; private set; } = true;
    public bool TwoFactorEnabled { get; private set; } = false;

    private UserSettings() { }

    public UserSettings(Guid userId)
    {
        UserId = userId;
    }

    public void UpdatePreferences(string? theme, string? language, string? timeZone)
    {
        if (!string.IsNullOrEmpty(theme)) Theme = theme;
        if (!string.IsNullOrEmpty(language)) Language = language;
        if (!string.IsNullOrEmpty(timeZone)) TimeZone = timeZone;
    }

    public void UpdateNotifications(bool email, bool sms, bool push)
    {
        EmailNotifications = email;
        SmsNotifications = sms;
        PushNotifications = push;
    }

    public void EnableTwoFactor()
    {
        TwoFactorEnabled = true;
    }

    public void DisableTwoFactor()
    {
        TwoFactorEnabled = false;
    }
}

public class OrganizationSettings : AuditableEntity
{
    public string SettingKey { get; private set; } = string.Empty;
    public string SettingValue { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;

    private OrganizationSettings() { }

    public OrganizationSettings(string key, string value, string description = "")
    {
        SettingKey = key;
        SettingValue = value;
        Description = description;
    }

    public void UpdateValue(string value)
    {
        SettingValue = value;
    }
}

public class HolidayCalendar : AuditableEntity
{
    public string HolidayName { get; private set; } = string.Empty;
    public DateTime HolidayDate { get; private set; }
    public string? Description { get; private set; }
    public bool IsOptional { get; private set; } = false;
    public int Year { get; private set; }

    private HolidayCalendar() { }

    public HolidayCalendar(string name, DateTime date, int year)
    {
        HolidayName = name;
        HolidayDate = date;
        Year = year;
    }
}

public class Shift : AuditableEntity
{
    public string ShiftName { get; private set; } = string.Empty;
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public int WorkingHours { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Shift() { }

    public Shift(string name, TimeSpan start, TimeSpan end, int hours)
    {
        ShiftName = name;
        StartTime = start;
        EndTime = end;
        WorkingHours = hours;
    }
}

public class WorkingHours : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public Guid ShiftId { get; private set; }
    public DateTime EffectiveFromUtc { get; private set; }
    public DateTime? EffectiveToUtc { get; private set; }
    public bool IsActive { get; private set; } = true;
    public Employee? Employee { get; private set; }
    public Shift? Shift { get; private set; }

    private WorkingHours() { }

    public WorkingHours(Guid employeeId, Guid shiftId)
    {
        EmployeeId = employeeId;
        ShiftId = shiftId;
        EffectiveFromUtc = DateTime.UtcNow;
    }

    public void EndAssignment()
    {
        IsActive = false;
        EffectiveToUtc = DateTime.UtcNow;
    }
}

public class EmailConfiguration : AuditableEntity
{
    public string SmtpServer { get; private set; } = string.Empty;
    public int SmtpPort { get; private set; }
    public string SenderEmail { get; private set; } = string.Empty;
    public string? SenderName { get; private set; }
    public bool EnableSsl { get; private set; } = true;
    public string? Username { get; private set; }
    public string? EncryptedPassword { get; private set; }

    private EmailConfiguration() { }

    public EmailConfiguration(string server, int port, string senderEmail)
    {
        SmtpServer = server;
        SmtpPort = port;
        SenderEmail = senderEmail;
    }

    public void UpdateConfiguration(string server, int port, string sender, bool ssl, string? username)
    {
        SmtpServer = server;
        SmtpPort = port;
        SenderEmail = sender;
        EnableSsl = ssl;
        Username = username;
    }
}
