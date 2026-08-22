using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Interfaces.Repositories;
using EmployeeLifecyclePortal.Domain.Entities;
using EmployeeLifecyclePortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EmployeeLifecyclePortal.Infrastructure.Services;

/// <summary>
/// Implementation of employee business logic including timelines and document management.
/// </summary>
public sealed class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        ApplicationDbContext context,
        IEmployeeRepository employeeRepository,
        IUnitOfWork unitOfWork,
        ILogger<EmployeeService> logger)
    {
        _context = context;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<EmployeeTimeline> AddTimelineEventAsync(
        Guid employeeId,
        string eventType,
        string title,
        string description,
        DateTime eventDateUtc,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);

        if (employee is null)
            throw new InvalidOperationException($"Employee with ID {employeeId} not found.");

        var timelineEvent = EmployeeTimeline.CreateEvent(
            employeeId,
            eventType,
            title,
            description,
            eventDateUtc,
            category);

        employee.AddTimelineEvent(timelineEvent);
        await _context.EmployeeTimelines.AddAsync(timelineEvent, cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Timeline event added — Employee: {EmployeeId} | Event: {EventType} | Title: {Title}",
            employeeId,
            eventType,
            title);

        return timelineEvent;
    }

    public async Task<List<EmployeeTimeline>> GetTimelineEventsAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var exists = await _context.Employees.AnyAsync(x => x.Id == employeeId, cancellationToken);
        if (!exists)
            throw new InvalidOperationException($"Employee with ID {employeeId} not found.");

        return await _context.EmployeeTimelines
            .Where(x => x.EmployeeId == employeeId)
            .OrderByDescending(x => x.EventDateUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeDocument> AddDocumentAsync(
        Guid employeeId,
        string documentType,
        string fileName,
        string filePath,
        string fileType,
        long fileSizeBytes,
        DateTime? expirationDateUtc = null,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);

        if (employee is null)
            throw new InvalidOperationException($"Employee with ID {employeeId} not found.");

        var document = EmployeeDocument.CreateDocument(
            employeeId,
            documentType,
            fileName,
            filePath,
            fileType,
            fileSizeBytes,
            expirationDateUtc,
            notes);

        employee.AddDocument(document);
        await _context.EmployeeDocuments.AddAsync(document, cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Document added — Employee: {EmployeeId} | Document: {FileName} | Type: {DocumentType}",
            employeeId,
            fileName,
            documentType);

        return document;
    }

    public async Task<List<EmployeeDocument>> GetDocumentsAsync(
        Guid employeeId,
        bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        var exists = await _context.Employees.AnyAsync(x => x.Id == employeeId, cancellationToken);
        if (!exists)
            throw new InvalidOperationException($"Employee with ID {employeeId} not found.");

        var query = _context.EmployeeDocuments.Where(x => x.EmployeeId == employeeId);

        if (!includeArchived)
        {
            query = query.Where(x => !x.IsArchived);
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task ArchiveDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var doc = await _context.EmployeeDocuments.FindAsync(new object[] { documentId }, cancellationToken);
        if (doc != null)
        {
            doc.Archive();
            await _unitOfWork.CommitAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Document archived — Document ID: {DocumentId}",
            documentId);
    }

    public async Task UnarchiveDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var doc = await _context.EmployeeDocuments.FindAsync(new object[] { documentId }, cancellationToken);
        if (doc != null)
        {
            doc.Unarchive();
            await _unitOfWork.CommitAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Document unarchived — Document ID: {DocumentId}",
            documentId);
    }

    public async Task<List<EmployeeDocument>> GetExpiringDocumentsAsync(
        int days,
        CancellationToken cancellationToken = default)
    {
        var threshold = DateTime.UtcNow.AddDays(days);
        return await _context.EmployeeDocuments
            .Where(d => !d.IsArchived && d.ExpirationDateUtc.HasValue && d.ExpirationDateUtc.Value <= threshold && d.ExpirationDateUtc.Value >= DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task AssignManagerAsync(
        Guid employeeId,
        Guid managerId,
        CancellationToken cancellationToken = default)
    {
        if (employeeId == managerId)
            throw new InvalidOperationException("An employee cannot be their own manager.");

        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);
        if (employee is null)
            throw new InvalidOperationException($"Employee with ID {employeeId} not found.");

        var manager = await _employeeRepository.GetByIdAsync(managerId, cancellationToken);
        if (manager is null)
            throw new InvalidOperationException($"Manager with ID {managerId} not found.");

        employee.AssignManager(managerId);

        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Manager assigned — Employee: {EmployeeId} | Manager: {ManagerId} | Manager Name: {ManagerName}",
            employeeId,
            managerId,
            manager.FullName);
    }

    public async Task RemoveManagerAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);

        if (employee is null)
            throw new InvalidOperationException($"Employee with ID {employeeId} not found.");

        employee.RemoveManager();

        await _unitOfWork.CommitAsync(cancellationToken);

        _logger.LogInformation(
            "Manager removed — Employee: {EmployeeId}",
            employeeId);
    }

    public async Task<List<Employee>> GetSubordinatesAsync(
        Guid managerId,
        CancellationToken cancellationToken = default)
    {
        var exists = await _context.Employees.AnyAsync(x => x.Id == managerId, cancellationToken);
        if (!exists)
            throw new InvalidOperationException($"Manager with ID {managerId} not found.");

        return await _context.Employees
            .Where(x => x.ManagerId == managerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Employee?> GetManagerAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var employee = await _context.Employees
            .Include(e => e.Manager)
            .FirstOrDefaultAsync(x => x.Id == employeeId, cancellationToken);

        return employee?.Manager;
    }
}
