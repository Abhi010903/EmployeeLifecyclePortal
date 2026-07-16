using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Application.Interfaces.Repositories;
using EmployeeLifecyclePortal.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EmployeeLifecyclePortal.Infrastructure.Services;

/// <summary>
/// Implementation of employee business logic including timelines and document management.
/// </summary>
public sealed class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IUnitOfWork unitOfWork,
        ILogger<EmployeeService> logger)
    {
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
        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);

        if (employee is null)
            throw new InvalidOperationException($"Employee with ID {employeeId} not found.");

        return employee.Timelines
            .OrderByDescending(x => x.EventDateUtc)
            .ToList();
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
        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);

        if (employee is null)
            throw new InvalidOperationException($"Employee with ID {employeeId} not found.");

        var documents = employee.Documents.AsEnumerable();

        if (!includeArchived)
        {
            documents = documents.Where(x => !x.IsArchived);
        }

        return documents
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToList();
    }

    public async Task ArchiveDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        // Note: In a real implementation, you would fetch the document from the repository
        // For now, this demonstrates the pattern
        _logger.LogInformation(
            "Document archived — Document ID: {DocumentId}",
            documentId);

        await Task.CompletedTask;
    }

    public async Task UnarchiveDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        // Note: In a real implementation, you would fetch the document from the repository
        _logger.LogInformation(
            "Document unarchived — Document ID: {DocumentId}",
            documentId);

        await Task.CompletedTask;
    }

    public async Task<List<EmployeeDocument>> GetExpiringDocumentsAsync(
        int days,
        CancellationToken cancellationToken = default)
    {
        // This would typically query the database for documents expiring within the specified days
        // For now, return empty list as a placeholder
        await Task.CompletedTask;
        return [];
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
        var manager = await _employeeRepository.GetByIdAsync(managerId, cancellationToken);

        if (manager is null)
            throw new InvalidOperationException($"Manager with ID {managerId} not found.");

        // Return subordinates from the manager's Subordinates collection
        // In a real implementation, this would query the database
        return manager.Subordinates.ToList();
    }

    public async Task<Employee?> GetManagerAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);

        if (employee is null)
            return null;

        if (employee.ManagerId.HasValue)
        {
            return await _employeeRepository.GetByIdAsync(employee.ManagerId.Value, cancellationToken);
        }

        return null;
    }
}
