using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Common;
using EmployeeLifecyclePortal.Domain.Entities;
using EmployeeLifecyclePortal.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Infrastructure.Persistence;

public sealed class ApplicationDbContext
    : DbContext,
      IApplicationDbContext
{
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUserService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IAuditService auditService,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _auditService = auditService;
        _currentUserService = currentUserService;
    }

    public DbSet<Employee> Employees => Set<Employee>();

    public DbSet<Department> Departments => Set<Department>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<EmployeeRole> EmployeeRoles => Set<EmployeeRole>();

    public DbSet<EmployeeTimeline> EmployeeTimelines => Set<EmployeeTimeline>();

    public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();

    public DbSet<ApplicationUser> ApplicationUsers
        => Set<ApplicationUser>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // Sprint 21: Attendance & Leave
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();

    // Sprint 23: Payroll
    public DbSet<SalaryStructure> SalaryStructures => Set<SalaryStructure>();
    public DbSet<SalaryComponent> SalaryComponents => Set<SalaryComponent>();
    public DbSet<Payslip> Payslips => Set<Payslip>();

    // Sprint 24: Recruitment
    public DbSet<JobPosting> JobPostings => Set<JobPosting>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<Interview> Interviews => Set<Interview>();
    public DbSet<JobOffer> JobOffers => Set<JobOffer>();

    // Sprint 25: Performance
    public DbSet<PerformanceGoal> PerformanceGoals => Set<PerformanceGoal>();
    public DbSet<PerformanceReview> PerformanceReviews => Set<PerformanceReview>();
    public DbSet<KPI> KPIs => Set<KPI>();

    // Sprint 26: Training
    public DbSet<TrainingCourse> TrainingCourses => Set<TrainingCourse>();
    public DbSet<EmployeeTraining> EmployeeTrainings => Set<EmployeeTraining>();
    public DbSet<Certification> Certifications => Set<Certification>();

    // Sprint 27: Assets
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetAssignment> AssetAssignments => Set<AssetAssignment>();
    public DbSet<AssetMaintenance> AssetMaintenances => Set<AssetMaintenance>();

    // Sprint 28: Notifications
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    // Sprint 29 & 30: Reporting & Dashboard
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<DashboardWidget> DashboardWidgets => Set<DashboardWidget>();
    public DbSet<DepartmentAnalytics> DepartmentAnalytics => Set<DepartmentAnalytics>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        // ── Record audit logs BEFORE committing changes ──────────────────────────
        // Capture all modifications (Add, Update, Delete) via the audit service
        var currentUserId = _currentUserService.GetCurrentUserId() ?? "system";

        var auditLogs = _auditService.GetAuditLogs(
            ChangeTracker.Entries(),
            currentUserId);

        // ── Set audit metadata on tracked entities ─────────────────────────────
        foreach (var entry in ChangeTracker
                     .Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.SetCreationAudit(currentUserId);
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetModificationAudit(currentUserId);
            }
        }

        // ── Add audit logs to the change tracker ───────────────────────────────
        // These will be persisted along with the other changes
        foreach (var auditLog in auditLogs)
        {
            AuditLogs.Add(auditLog);
        }

        return await base.SaveChangesAsync(
            cancellationToken);
    }
}