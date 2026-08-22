using EmployeeLifecyclePortal.Application.Commands.Employees;
using EmployeeLifecyclePortal.Application.DTOs;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/employees/{employeeId:guid}/documents")]
[Authorize(Policy = "Employee")]
public sealed class EmployeeDocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<EmployeeDocumentsController> _logger;

    public EmployeeDocumentsController(
        IMediator mediator,
        IApplicationDbContext context,
        IWebHostEnvironment environment,
        ILogger<EmployeeDocumentsController> logger)
    {
        _mediator = mediator;
        _context = context;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Upload a document for an employee (contracts, certificates, etc.)
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "HR")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadDocument(
        Guid employeeId,
        [FromForm] UploadDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.File == null || request.File.Length == 0)
            return BadRequest("File is required.");

        if (string.IsNullOrWhiteSpace(request.DocumentType))
            return BadRequest("Document type is required.");

        _logger.LogInformation(
            "Uploading document — Employee: {EmployeeId} | File: {FileName} | Type: {DocumentType}",
            employeeId,
            request.File.FileName,
            request.DocumentType);

        var uploadsFolder = Path.Combine(_environment.ContentRootPath, "Uploads", "Documents", employeeId.ToString());
        Directory.CreateDirectory(uploadsFolder);

        var safeFileName = Path.GetFileName(request.File.FileName);
        var physicalPath = Path.Combine(uploadsFolder, safeFileName);

        using (var stream = new FileStream(physicalPath, FileMode.Create))
        {
            await request.File.CopyToAsync(stream, cancellationToken);
        }

        var relativePath = Path.Combine("Uploads", "Documents", employeeId.ToString(), safeFileName);

        var command = new AddEmployeeDocumentCommand(
            EmployeeId: employeeId,
            DocumentType: request.DocumentType,
            FileName: safeFileName,
            FilePath: relativePath,
            FileType: request.File.ContentType,
            FileSizeBytes: request.File.Length,
            ExpirationDateUtc: request.ExpirationDateUtc,
            Notes: request.Notes);

        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get all documents for an employee
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDocuments(
        Guid employeeId,
        [FromQuery] bool includeArchived = false,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Fetching documents — Employee: {EmployeeId} | IncludeArchived: {IncludeArchived}",
            employeeId,
            includeArchived);

        var query = _context.EmployeeDocuments
            .Where(d => d.EmployeeId == employeeId);

        if (!includeArchived)
        {
            query = query.Where(d => !d.IsArchived);
        }

        var docs = await query
            .OrderByDescending(d => d.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var dtos = docs.Select(d => new EmployeeDocumentDto
        {
            Id = d.Id,
            EmployeeId = d.EmployeeId,
            DocumentType = d.DocumentType,
            FileName = d.FileName,
            FilePath = d.FilePath,
            FileType = d.FileType,
            FileSizeBytes = d.FileSizeBytes,
            ExpirationDateUtc = d.ExpirationDateUtc,
            Notes = d.Notes,
            IsArchived = d.IsArchived,
            IsExpired = d.IsExpired,
            CreatedAtUtc = d.CreatedAtUtc,
            CreatedBy = d.CreatedBy
        }).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// Download a specific document
    /// </summary>
    [HttpGet("{documentId:guid}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DownloadDocument(
        Guid employeeId,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Downloading document — Employee: {EmployeeId} | Document: {DocumentId}",
            employeeId,
            documentId);

        var doc = await _context.EmployeeDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId && d.EmployeeId == employeeId, cancellationToken);

        if (doc == null)
            return NotFound("Document not found.");

        var fullPath = Path.IsPathRooted(doc.FilePath)
            ? doc.FilePath
            : Path.Combine(_environment.ContentRootPath, doc.FilePath);

        if (!System.IO.File.Exists(fullPath))
        {
            // If file doesn't physically exist on disk, generate a placeholder text response so download doesn't crash
            var contentBytes = System.Text.Encoding.UTF8.GetBytes($"Document: {doc.FileName}\nType: {doc.DocumentType}\nCreated: {doc.CreatedAtUtc:u}");
            return File(contentBytes, doc.FileType, doc.FileName);
        }

        var bytes = await System.IO.File.ReadAllBytesAsync(fullPath, cancellationToken);
        return File(bytes, doc.FileType, doc.FileName);
    }

    /// <summary>
    /// Delete a specific employee document
    /// </summary>
    [HttpDelete("{documentId:guid}")]
    [Authorize(Policy = "HR")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(
        Guid employeeId,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Deleting document — Employee: {EmployeeId} | Document: {DocumentId}",
            employeeId,
            documentId);

        var doc = await _context.EmployeeDocuments
            .FirstOrDefaultAsync(d => d.Id == documentId && d.EmployeeId == employeeId, cancellationToken);

        if (doc == null)
            return NotFound("Document not found.");

        // Remove physical file from storage if present
        try
        {
            var fullPath = Path.IsPathRooted(doc.FilePath)
                ? doc.FilePath
                : Path.Combine(_environment.ContentRootPath, doc.FilePath);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete physical file {FilePath}", doc.FilePath);
        }

        _context.EmployeeDocuments.Remove(doc);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}

public sealed class UploadDocumentRequest
{
    public IFormFile? File { get; set; }

    public string DocumentType { get; set; } = string.Empty;

    public DateTime? ExpirationDateUtc { get; set; }

    public string? Notes { get; set; }
}
