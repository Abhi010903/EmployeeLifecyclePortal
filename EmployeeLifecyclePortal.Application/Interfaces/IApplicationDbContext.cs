using EmployeeLifecyclePortal.Domain.Entities;
using EmployeeLifecyclePortal.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Employee> Employees { get; }

    DbSet<Department> Departments { get; }

    DbSet<Role> Roles { get; }

    DbSet<EmployeeRole> EmployeeRoles { get; }

    DbSet<ApplicationUser> ApplicationUsers { get; }

    DbSet<AuditLog> AuditLogs { get; }

    DbSet<Attendance> Attendances { get; }

    DbSet<LeaveType> LeaveTypes { get; }

    DbSet<LeaveRequest> LeaveRequests { get; }

    DbSet<LeaveBalance> LeaveBalances { get; }

    DbSet<Payslip> Payslips { get; }

    DbSet<SalaryStructure> SalaryStructures { get; }

    DbSet<SalaryComponent> SalaryComponents { get; }

    DbSet<Reimbursement> Reimbursements { get; }

    // Tasks and Staffing
    DbSet<WorkTask> WorkTasks { get; }

    DbSet<StaffingRequest> StaffingRequests { get; }

    // Recruitment
    DbSet<JobPosting> JobPostings { get; }

    DbSet<Candidate> Candidates { get; }

    DbSet<Interview> Interviews { get; }

    DbSet<JobOffer> JobOffers { get; }

    // Employee related
    DbSet<EmployeeTimeline> EmployeeTimelines { get; }

    DbSet<EmployeeDocument> EmployeeDocuments { get; }

    // Performance
    DbSet<PerformanceGoal> PerformanceGoals { get; }

    DbSet<PerformanceReview> PerformanceReviews { get; }

    DbSet<KPI> KPIs { get; }

    // Assets
    DbSet<Domain.Entities.Asset> Assets { get; }

    DbSet<AssetAssignment> AssetAssignments { get; }

    DbSet<AssetMaintenance> AssetMaintenances { get; }

    // Reports
    DbSet<Report> Reports { get; }

    DbSet<DashboardWidget> DashboardWidgets { get; }

    DbSet<DepartmentAnalytics> DepartmentAnalytics { get; }

    // Settings
    DbSet<CompanyProfile> CompanyProfiles { get; }

    DbSet<UserSettings> UserSettings { get; }

    DbSet<OrganizationSettings> OrganizationSettings { get; }

    DbSet<HolidayCalendar> HolidayCalendars { get; }

    DbSet<Shift> Shifts { get; }

    DbSet<WorkingHours> WorkingHours { get; }

    DbSet<EmailConfiguration> EmailConfigurations { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}