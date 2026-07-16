using EmployeeLifecyclePortal.Domain.Entities;

namespace EmployeeLifecyclePortal.Application.Interfaces;

/// <summary>
/// Service for employee business logic including timelines and document management
/// </summary>
public interface IEmployeeService
{
    /// <summary>
    /// Adds a timeline event to an employee
    /// </summary>
    Task<EmployeeTimeline> AddTimelineEventAsync(
        Guid employeeId,
        string eventType,
        string title,
        string description,
        DateTime eventDateUtc,
        string? category = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all timeline events for an employee, ordered by date descending
    /// </summary>
    Task<List<EmployeeTimeline>> GetTimelineEventsAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a document for an employee
    /// </summary>
    Task<EmployeeDocument> AddDocumentAsync(
        Guid employeeId,
        string documentType,
        string fileName,
        string filePath,
        string fileType,
        long fileSizeBytes,
        DateTime? expirationDateUtc = null,
        string? notes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all documents for an employee
    /// </summary>
    Task<List<EmployeeDocument>> GetDocumentsAsync(
        Guid employeeId,
        bool includeArchived = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Archives a document
    /// </summary>
    Task ArchiveDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unarchives a document
    /// </summary>
    Task UnarchiveDocumentAsync(
        Guid documentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets documents expiring within a specified number of days
    /// </summary>
    Task<List<EmployeeDocument>> GetExpiringDocumentsAsync(
        int days,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a manager to an employee
    /// </summary>
    Task AssignManagerAsync(
        Guid employeeId,
        Guid managerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the manager assignment from an employee
    /// </summary>
    Task RemoveManagerAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all subordinates (direct reports) of an employee
    /// </summary>
    Task<List<Employee>> GetSubordinatesAsync(
        Guid managerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the direct manager of an employee
    /// </summary>
    Task<Employee?> GetManagerAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default);
}
