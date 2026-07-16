using EmployeeLifecyclePortal.Application.DTOs;
using MediatR;

namespace EmployeeLifecyclePortal.Application.Commands.Employees;

public sealed record AddEmployeeDocumentCommand(
    Guid EmployeeId,
    string DocumentType,
    string FileName,
    string FilePath,
    string FileType,
    long FileSizeBytes,
    DateTime? ExpirationDateUtc,
    string? Notes)
    : IRequest<EmployeeDocumentDto>;
