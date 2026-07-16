using EmployeeLifecyclePortal.Application.Commands.Employees;
using EmployeeLifecyclePortal.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/employees/{employeeId}/documents")]
[Authorize(Policy = "HR")]
public sealed class EmployeeDocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EmployeeDocumentsController> _logger;

    public EmployeeDocumentsController(
        IMediator mediator,
        ILogger<EmployeeDocumentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Upload a document for an employee (contracts, certificates, etc.)
    /// </summary>
    [HttpPost]
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

        // In a real implementation, save file to storage and get the path
        var filePath = $"documents/{employeeId}/{request.File.FileName}";

        var command = new AddEmployeeDocumentCommand(
            EmployeeId: employeeId,
            DocumentType: request.DocumentType,
            FileName: request.File.FileName,
            FilePath: filePath,
            FileType: request.File.ContentType,
            FileSizeBytes: request.File.Length,
            ExpirationDateUtc: request.ExpirationDateUtc,
            Notes: request.Notes);

        var result = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(UploadDocument), new { employeeId, documentId = result.Id }, result);
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

        // This would be implemented via a query handler in production
        return Ok(new List<EmployeeDocumentDto>());
    }

    /// <summary>
    /// Download a specific document
    /// </summary>
    [HttpGet("{documentId}/download")]
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

        // This would be implemented to retrieve file from storage
        return NotFound("Document not found.");
    }
}

public sealed class UploadDocumentRequest
{
    public IFormFile? File { get; set; }

    public string DocumentType { get; set; } = string.Empty;

    public DateTime? ExpirationDateUtc { get; set; }

    public string? Notes { get; set; }
}
