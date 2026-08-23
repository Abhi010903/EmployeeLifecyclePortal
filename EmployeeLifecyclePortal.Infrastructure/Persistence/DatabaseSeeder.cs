using EmployeeLifecyclePortal.Application.Services.Auth;
using EmployeeLifecyclePortal.Domain.Entities;
using EmployeeLifecyclePortal.Domain.Entities.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EmployeeLifecyclePortal.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task InitializeDatabaseAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetService<ILogger<ApplicationDbContext>>();

        try
        {
            // Ensure database, all required tables, and all columns exist in SQL Server
            await EnsureSchemaCreatedAsync(context, logger);

            // Seed Admin User
            if (!await context.ApplicationUsers.AnyAsync(u => u.Email == "admin@example.com"))
            {
                var passwordHash = passwordHasher.Hash("Admin@123456");
                var adminUser = new ApplicationUser(
                    username: "System Admin",
                    email: "admin@example.com",
                    passwordHash: passwordHash,
                    role: "Admin");

                context.ApplicationUsers.Add(adminUser);
            }

            // Seed Roles
            var defaultRoles = new[]
            {
                ("Admin", "Full administrative access to all portal modules and settings"),
                ("HR", "Human Resources manager with employee, onboarding, and payroll access"),
                ("Manager", "Team manager with approval and performance management permissions"),
                ("Team Lead", "Team leader supervising direct reports and daily tasks"),
                ("Employee", "Standard employee access for self-service, attendance, and leave")
            };

            foreach (var (name, description) in defaultRoles)
            {
                if (!await context.Roles.AnyAsync(r => r.Name == name))
                {
                    context.Roles.Add(new Role(name, description));
                }
            }

            // Seed Departments
            var defaultDepartments = new[]
            {
                ("Engineering", "Software development, QA, architecture, and technology infrastructure"),
                ("Human Resources", "People operations, talent acquisition, and employee relations"),
                ("Finance", "Financial planning, accounting, payroll, and auditing"),
                ("Marketing", "Product marketing, brand strategy, and customer outreach"),
                ("Operations", "Business operations, logistics, and facilities management")
            };

            foreach (var (name, description) in defaultDepartments)
            {
                if (!await context.Departments.AnyAsync(d => d.Name == name))
                {
                    context.Departments.Add(new Department(name, description));
                }
            }

            // Seed Leave Types
            if (!await context.LeaveTypes.AnyAsync())
            {
                context.LeaveTypes.AddRange(
                    new LeaveType("Annual Leave", 20, true, "Paid vacation leave"),
                    new LeaveType("Sick Leave", 12, true, "Paid leave for health issues"),
                    new LeaveType("Casual Leave", 6, true, "Short-notice personal leave"),
                    new LeaveType("Maternity/Paternity", 90, true, "Parental leave for new parents")
                );
            }

            // Seed Company Profile
            if (!await context.CompanyProfiles.AnyAsync())
            {
                var profile = new CompanyProfile("Employee Lifecycle Portal");
                profile.UpdateProfile(
                    name: "Employee Lifecycle Portal",
                    code: "ELP-001",
                    industry: "Technology & Software",
                    address: "Tech Park, Innovation Boulevard",
                    city: "Bengaluru",
                    state: "Karnataka",
                    country: "India",
                    postal: "560100",
                    phone: "+91 80 1234 5678",
                    email: "hr@employeeportal.com",
                    website: "https://employeeportal.internal",
                    registration: "REG-2026-IN-001",
                    founded: new DateTime(2020, 1, 1)
                );
                context.CompanyProfiles.Add(profile);
            }

            // Seed Shifts
            if (!await context.Shifts.AnyAsync())
            {
                context.Shifts.AddRange(
                    new Shift("General Shift", new TimeSpan(9, 0, 0), new TimeSpan(18, 0, 0), 60),
                    new Shift("Morning Shift", new TimeSpan(6, 0, 0), new TimeSpan(15, 0, 0), 60),
                    new Shift("Evening Shift", new TimeSpan(14, 0, 0), new TimeSpan(23, 0, 0), 60)
                );
            }

            // Seed Indian Holidays
            if (!await context.HolidayCalendars.AnyAsync())
            {
                var currentYear = DateTime.UtcNow.Year;
                context.HolidayCalendars.AddRange(
                    new HolidayCalendar("Republic Day", new DateTime(currentYear, 1, 26), currentYear),
                    new HolidayCalendar("Holi", new DateTime(currentYear, 3, 25), currentYear),
                    new HolidayCalendar("Independence Day", new DateTime(currentYear, 8, 15), currentYear),
                    new HolidayCalendar("Mahatma Gandhi Jayanti", new DateTime(currentYear, 10, 2), currentYear),
                    new HolidayCalendar("Diwali", new DateTime(currentYear, 11, 1), currentYear),
                    new HolidayCalendar("Christmas", new DateTime(currentYear, 12, 25), currentYear)
                );
            }

            // Seed default SalaryStructure for employees if missing
            var employees = await context.Employees.ToListAsync();
            foreach (var emp in employees)
            {
                if (!await context.SalaryStructures.AnyAsync(s => s.EmployeeId == emp.Id))
                {
                    decimal baseSal = emp.FirstName.Contains("Vivek") ? 85000m :
                                      emp.FirstName.Contains("Abhimanyu") ? 95000m :
                                      emp.FirstName.Contains("Bheem") ? 80000m : 70000m;
                    context.SalaryStructures.Add(new SalaryStructure(emp.Id, baseSal, new DateTime(2026, 1, 1)));
                }
            }

            // Ensure Employee Rahul Sharma exists
            var engDept = await context.Departments.FirstOrDefaultAsync(d => d.Name == "Engineering")
                ?? await context.Departments.FirstOrDefaultAsync();

            var rahulEmployee = await context.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == "EMP-276595" || e.Email.Contains("rahul"));
            if (rahulEmployee == null && engDept != null)
            {
                rahulEmployee = new Employee(
                    employeeCode: "EMP-276595",
                    firstName: "Rahul",
                    lastName: "Sharma",
                    email: "rahul.sharma.test@example.com",
                    phoneNumber: "+91 98765 43210",
                    departmentId: engDept.Id
                );
                context.Employees.Add(rahulEmployee);
                await context.SaveChangesAsync();

                context.SalaryStructures.Add(new SalaryStructure(rahulEmployee.Id, 72000m, new DateTime(2026, 1, 1)));
            }

            // Seed / Update Employee User account (Rahul Sharma)
            var empUser = await context.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == "employee@example.com");
            if (empUser == null)
            {
                var passwordHash = passwordHasher.Hash("Employee@123456");
                empUser = new ApplicationUser(
                    username: "Rahul Sharma",
                    email: "employee@example.com",
                    passwordHash: passwordHash,
                    role: "Employee",
                    employeeId: rahulEmployee?.Id);

                context.ApplicationUsers.Add(empUser);
            }
            else if (rahulEmployee != null && (!empUser.EmployeeId.HasValue || empUser.EmployeeId.Value == Guid.Empty))
            {
                empUser.LinkEmployee(rahulEmployee.Id);
            }

            // Seed Manager User account (Vivek Singh)
            var vivekEmployee = await context.Employees.FirstOrDefaultAsync(e => e.FirstName.Contains("Vivek"));
            var managerUser = await context.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == "manager@example.com");
            if (managerUser == null)
            {
                var passwordHash = passwordHasher.Hash("Manager@123456");
                managerUser = new ApplicationUser(
                    username: "Vivek Singh",
                    email: "manager@example.com",
                    passwordHash: passwordHash,
                    role: "Manager",
                    employeeId: vivekEmployee?.Id);

                context.ApplicationUsers.Add(managerUser);
            }
            else if (vivekEmployee != null && (!managerUser.EmployeeId.HasValue || managerUser.EmployeeId.Value == Guid.Empty))
            {
                managerUser.LinkEmployee(vivekEmployee.Id);
            }

            // Auto-link any existing users without EmployeeId
            var allUsers = await context.ApplicationUsers.ToListAsync();
            var allEmps = await context.Employees.ToListAsync();
            foreach (var u in allUsers.Where(u => !u.EmployeeId.HasValue))
            {
                var matched = allEmps.FirstOrDefault(e => e.Email.Equals(u.Email, StringComparison.OrdinalIgnoreCase) ||
                                                          u.Username.Contains(e.FirstName, StringComparison.OrdinalIgnoreCase));
                if (matched != null)
                {
                    u.LinkEmployee(matched.Id);
                }
            }

            // Seed sample payslips for Rahul if none exist
            if (rahulEmployee != null && !await context.Payslips.AnyAsync(p => p.EmployeeId == rahulEmployee.Id))
            {
                var curMonth = DateTime.UtcNow.Month;
                var curYear = DateTime.UtcNow.Year;

                // Last month payslip
                var prevMonth = curMonth > 1 ? curMonth - 1 : 12;
                var prevYear = curMonth > 1 ? curYear : curYear - 1;

                var prevPayslip = Payslip.CreateDetailed(
                    employeeId: rahulEmployee.Id,
                    month: prevMonth,
                    year: prevYear,
                    basicSalary: 36000m,
                    hra: 21600m,
                    allowances: 14400m,
                    bonusPay: 0m,
                    overtimePay: 0m,
                    pfDeduction: 4320m,
                    esiDeduction: 540m,
                    tdsDeduction: 3600m,
                    reimbursementsAmount: 0m,
                    workingDays: 30,
                    presentDays: 28,
                    paidLeaveDays: 2,
                    unpaidLeaveDays: 0,
                    status: "Paid",
                    remarks: "Salary credited via Direct Bank Transfer"
                );
                context.Payslips.Add(prevPayslip);

                // Current month payslip
                var curPayslip = Payslip.CreateDetailed(
                    employeeId: rahulEmployee.Id,
                    month: curMonth,
                    year: curYear,
                    basicSalary: 36000m,
                    hra: 21600m,
                    allowances: 14400m,
                    bonusPay: 5000m,
                    overtimePay: 0m,
                    pfDeduction: 4320m,
                    esiDeduction: 540m,
                    tdsDeduction: 4100m,
                    reimbursementsAmount: 0m,
                    workingDays: 30,
                    presentDays: 30,
                    paidLeaveDays: 0,
                    unpaidLeaveDays: 0,
                    status: "Paid",
                    remarks: "Regular monthly payroll"
                );
                context.Payslips.Add(curPayslip);
            }

            // Seed sample assets for Rahul if none exist
            if (rahulEmployee != null && !await context.AssetAssignments.AnyAsync(aa => aa.EmployeeId == rahulEmployee.Id))
            {
                var laptop = await context.Assets.FirstOrDefaultAsync(a => a.AssetName.Contains("MacBook") || a.AssetName.Contains("Dell") || a.AssetName.Contains("Laptop"));
                if (laptop == null)
                {
                    laptop = new Asset("DELL-XPS-001", "Dell XPS 15 (Dev Workstation)", "Hardware", "DELL-XPS-2026-042", 145000m, new DateTime(2026, 1, 15));
                    context.Assets.Add(laptop);
                    await context.SaveChangesAsync();
                }

                var assignment = new AssetAssignment(rahulEmployee.Id, laptop.Id);
                assignment.Notes = "Primary developer workstation with accessories";
                context.AssetAssignments.Add(assignment);
            }

            // Seed sample reimbursement for Rahul if none exist
            if (rahulEmployee != null && !await context.Reimbursements.AnyAsync(r => r.EmployeeId == rahulEmployee.Id))
            {
                var reimb = new Reimbursement(
                    employeeId: rahulEmployee.Id,
                    amount: 2450m,
                    category: "Broadband",
                    description: "High-speed home internet allowance for remote development",
                    receiptUrl: null
                );
                reimb.Approve(Guid.NewGuid());
                context.Reimbursements.Add(reimb);

                var reimb2 = new Reimbursement(
                    employeeId: rahulEmployee.Id,
                    amount: 1800m,
                    category: "Travel",
                    description: "Client site technical discussion taxi fare",
                    receiptUrl: null
                );
                context.Reimbursements.Add(reimb2);
            }

            // Seed sample tasks for Rahul if none exist
            if (rahulEmployee != null && !await context.WorkTasks.AnyAsync(t => t.EmployeeId == rahulEmployee.Id))
            {
                var task1 = new WorkTask(
                    title: "Employee Self-Service Optimization",
                    description: "Enhance self-service dashboard widgets, payslip PDF download and leave tracking interface",
                    employeeId: rahulEmployee.Id,
                    departmentId: rahulEmployee.DepartmentId,
                    managerId: rahulEmployee.ManagerId,
                    priority: "High",
                    startDateUtc: DateTime.UtcNow.AddDays(-3),
                    deadlineUtc: DateTime.UtcNow.AddDays(5)
                );
                task1.UpdateProgress(70, "InProgress");
                context.WorkTasks.Add(task1);

                var task2 = new WorkTask(
                    title: "API Authorization Audit",
                    description: "Verify role-based permission checks and IDOR protection on all domain controllers",
                    employeeId: rahulEmployee.Id,
                    departmentId: rahulEmployee.DepartmentId,
                    managerId: rahulEmployee.ManagerId,
                    priority: "Medium",
                    startDateUtc: DateTime.UtcNow.AddDays(-1),
                    deadlineUtc: DateTime.UtcNow.AddDays(7)
                );
                task2.UpdateProgress(100, "Completed");
                context.WorkTasks.Add(task2);
            }

            // Seed sample leave balances for Rahul
            if (rahulEmployee != null && !await context.LeaveBalances.AnyAsync(lb => lb.EmployeeId == rahulEmployee.Id))
            {
                var leaveTypes = await context.LeaveTypes.ToListAsync();
                var curYear = DateTime.UtcNow.Year;
                foreach (var lt in leaveTypes)
                {
                    var used = lt.Name.Contains("Annual") ? 2 : (lt.Name.Contains("Sick") ? 1 : 0);
                    context.LeaveBalances.Add(new LeaveBalance(rahulEmployee.Id, lt.Id, lt.DaysPerYear, used, curYear));
                }
            }

            await context.SaveChangesAsync();
            logger?.LogInformation("Database initialized and seed data successfully verified.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    private static async Task EnsureSchemaCreatedAsync(ApplicationDbContext context, ILogger? logger)
    {
        var databaseCreator = context.Database.GetService<IRelationalDatabaseCreator>();
        if (!await databaseCreator.ExistsAsync())
        {
            logger?.LogInformation("Database does not exist. Creating database and all tables...");
            await context.Database.EnsureCreatedAsync();
            return;
        }

        // Database exists; verify all entity tables exist in SQL Server
        var entityTypes = context.Model.GetEntityTypes();
        var existingTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var existingColumnsByTable = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        var connection = context.Database.GetDbConnection();
        if (string.IsNullOrEmpty(connection.ConnectionString))
        {
            connection.ConnectionString = context.Database.GetConnectionString()
                ?? "Server=(localdb)\\MSSQLLocalDB;Database=EmployeeLifecyclePortalDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
        }

        await context.Database.OpenConnectionAsync();
        try
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                existingTables.Add(reader.GetString(0));
            }
            reader.Close();

            using var colCmd = connection.CreateCommand();
            colCmd.CommandText = "SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS";
            using var colReader = await colCmd.ExecuteReaderAsync();
            while (await colReader.ReadAsync())
            {
                var tbl = colReader.GetString(0);
                var col = colReader.GetString(1);
                if (!existingColumnsByTable.TryGetValue(tbl, out var set))
                {
                    set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    existingColumnsByTable[tbl] = set;
                }
                set.Add(col);
            }
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }

        var missingTables = new List<string>();
        foreach (var entityType in entityTypes)
        {
            var tableName = entityType.GetTableName();
            if (!string.IsNullOrEmpty(tableName) && !existingTables.Contains(tableName))
            {
                missingTables.Add(tableName);
            }
        }

        if (missingTables.Count > 0)
        {
            logger?.LogInformation("Found {Count} missing tables ({Tables}). Applying schema create script...",
                missingTables.Count, string.Join(", ", missingTables));

            var createScript = context.Database.GenerateCreateScript();
            var commands = createScript.Split(new[] { "\r\n\r\n", "\n\n", "GO\r\n", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var rawSql in commands)
            {
                var sql = rawSql.Trim();
                if (string.IsNullOrWhiteSpace(sql)) continue;

                try
                {
                    await context.Database.ExecuteSqlRawAsync(sql);
                }
                catch (Exception ex)
                {
                    logger?.LogDebug("Schema creation note: {Message}", ex.Message);
                }
            }

            logger?.LogInformation("Schema table synchronization completed successfully.");
        }

        // Check for missing columns in existing tables and add them safely
        foreach (var entityType in entityTypes)
        {
            var tableName = entityType.GetTableName();
            if (string.IsNullOrEmpty(tableName) || !existingColumnsByTable.TryGetValue(tableName, out var cols))
            {
                continue;
            }

            foreach (var property in entityType.GetProperties())
            {
                var colName = property.GetColumnName();
                if (!string.IsNullOrEmpty(colName) && !cols.Contains(colName))
                {
                    var clrType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                    string sqlType = "nvarchar(MAX)";
                    if (clrType == typeof(Guid)) sqlType = "uniqueidentifier";
                    else if (clrType == typeof(DateTime)) sqlType = "datetime2";
                    else if (clrType == typeof(int) || clrType.IsEnum) sqlType = "int";
                    else if (clrType == typeof(decimal)) sqlType = "decimal(18,2)";
                    else if (clrType == typeof(bool)) sqlType = "bit";

                    logger?.LogInformation("Adding missing column {Column} ({Type}) to table {Table}...", colName, sqlType, tableName);
                    try
                    {
                        var alterSql = string.Format("ALTER TABLE [{0}] ADD [{1}] {2} NULL;", tableName, colName, sqlType);
                        await context.Database.ExecuteSqlRawAsync(alterSql);
                        cols.Add(colName);
                    }
                    catch (Exception ex)
                    {
                        logger?.LogWarning(ex, "Failed to add column {Column} to table {Table}", colName, tableName);
                    }
                }
            }
        }
    }
}
