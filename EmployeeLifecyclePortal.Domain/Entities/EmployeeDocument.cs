using EmployeeLifecyclePortal.Domain.Common;

namespace EmployeeLifecyclePortal.Domain.Entities;

/// <summary>
/// Stores employee documents like contracts, certificates, training records, offer letters, payslips, etc.
/// Documents are immutable after creation.
/// </summary>
public class EmployeeDocument : AuditableEntity
{
    public Guid EmployeeId { get; private set; }

    /// <summary>
    /// Type of document: "Contract", "Certificate", "OfferLetter", "Payslip", "TrainingRecord", "Evaluation", "Other"
    /// </summary>
    public string DocumentType { get; private set; } = string.Empty;

    /// <summary>
    /// Display name/title of the document
    /// </summary>
    public string FileName { get; private set; } = string.Empty;

    /// <summary>
    /// File content stored as base64 encoded string or file path reference
    /// </summary>
    public string FilePath { get; private set; } = string.Empty;

    /// <summary>
    /// MIME type of the document (e.g., "application/pdf", "application/msword")
    /// </summary>
    public string FileType { get; private set; } = string.Empty;

    /// <summary>
    /// Size of the file in bytes
    /// </summary>
    public long FileSizeBytes { get; private set; }

    /// <summary>
    /// Optional expiration date for documents like certificates or training records
    /// </summary>
    public DateTime? ExpirationDateUtc { get; private set; }

    /// <summary>
    /// Additional notes or metadata about the document
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Whether the document has been archived
    /// </summary>
    public bool IsArchived { get; private set; }

    /// <summary>
    /// Navigation property to the employee
    /// </summary>
    public Employee? Employee { get; private set; }

    private EmployeeDocument()
    {
    }

    /// <summary>
    /// Creates a new document record for an employee.
    /// </summary>
    public static EmployeeDocument CreateDocument(
        Guid employeeId,
        string documentType,
        string fileName,
        string filePath,
        string fileType,
        long fileSizeBytes,
        DateTime? expirationDateUtc = null,
        string? notes = null)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee ID is required.", nameof(employeeId));

        if (string.IsNullOrWhiteSpace(documentType))
            throw new ArgumentException("Document type is required.", nameof(documentType));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required.", nameof(fileName));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path is required.", nameof(filePath));

        if (fileSizeBytes <= 0)
            throw new ArgumentException("File size must be greater than 0.", nameof(fileSizeBytes));

        var validDocumentTypes = new[] { "Contract", "Certificate", "OfferLetter", "Payslip", "TrainingRecord", "Evaluation", "Other" };

        if (!validDocumentTypes.Contains(documentType))
            throw new ArgumentException(
                $"Document type must be one of: {string.Join(", ", validDocumentTypes)}",
                nameof(documentType));

        return new EmployeeDocument
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            DocumentType = documentType,
            FileName = fileName,
            FilePath = filePath,
            FileType = fileType,
            FileSizeBytes = fileSizeBytes,
            ExpirationDateUtc = expirationDateUtc,
            Notes = notes,
            IsArchived = false,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void Archive()
    {
        IsArchived = true;
    }

    public void Unarchive()
    {
        IsArchived = false;
    }

    public bool IsExpired => ExpirationDateUtc.HasValue && ExpirationDateUtc <= DateTime.UtcNow;

    public bool ExpiringWithin(int days)
    {
        if (!ExpirationDateUtc.HasValue)
            return false;

        var expiresIn = ExpirationDateUtc.Value - DateTime.UtcNow;
        return expiresIn.TotalDays <= days && expiresIn.TotalDays >= 0;
    }
}
