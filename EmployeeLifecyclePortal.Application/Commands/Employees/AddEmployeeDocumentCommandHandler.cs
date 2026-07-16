using EmployeeLifecyclePortal.Application.DTOs;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EmployeeLifecyclePortal.Application.Commands.Employees;

public sealed class AddEmployeeDocumentCommandHandler
    : IRequestHandler<AddEmployeeDocumentCommand, EmployeeDocumentDto>
{
    private readonly IEmployeeService _employeeService;
    private readonly ILogger<AddEmployeeDocumentCommandHandler> _logger;

    public AddEmployeeDocumentCommandHandler(
        IEmployeeService employeeService,
        ILogger<AddEmployeeDocumentCommandHandler> logger)
    {
        _employeeService = employeeService;
        _logger = logger;
    }

    public async Task<EmployeeDocumentDto> Handle(
        AddEmployeeDocumentCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Adding document — Employee: {EmployeeId} | Document: {FileName} | Type: {DocumentType}",
            request.EmployeeId,
            request.FileName,
            request.DocumentType);

        var document = await _employeeService.AddDocumentAsync(
            request.EmployeeId,
            request.DocumentType,
            request.FileName,
            request.FilePath,
            request.FileType,
            request.FileSizeBytes,
            request.ExpirationDateUtc,
            request.Notes,
            cancellationToken);

        _logger.LogInformation(
            "Document added successfully — Employee: {EmployeeId} | Document ID: {DocumentId}",
            request.EmployeeId,
            document.Id);

        return new EmployeeDocumentDto
        {
            Id = document.Id,
            EmployeeId = document.EmployeeId,
            DocumentType = document.DocumentType,
            FileName = document.FileName,
            FilePath = document.FilePath,
            FileType = document.FileType,
            FileSizeBytes = document.FileSizeBytes,
            ExpirationDateUtc = document.ExpirationDateUtc,
            Notes = document.Notes,
            IsArchived = document.IsArchived,
            IsExpired = document.IsExpired,
            CreatedAtUtc = document.CreatedAtUtc,
            CreatedBy = document.CreatedBy
        };
    }
}
