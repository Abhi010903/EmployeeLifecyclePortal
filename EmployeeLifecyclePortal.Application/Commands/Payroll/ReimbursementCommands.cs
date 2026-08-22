using EmployeeLifecyclePortal.Application.DTOs.Payroll;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Payroll;

public sealed record CreateReimbursementCommand(
    Guid EmployeeId,
    decimal Amount,
    string Category,
    string Description,
    string? ReceiptUrl) : IRequest<ReimbursementDto>;

public sealed class CreateReimbursementCommandHandler : IRequestHandler<CreateReimbursementCommand, ReimbursementDto>
{
    private readonly IApplicationDbContext _context;

    public CreateReimbursementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ReimbursementDto> Handle(CreateReimbursementCommand request, CancellationToken cancellationToken)
    {
        var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);
        if (employee == null)
            throw new InvalidOperationException("Employee not found.");

        var reimbursement = new Reimbursement(
            request.EmployeeId,
            request.Amount,
            request.Category,
            request.Description,
            request.ReceiptUrl);

        _context.Reimbursements.Add(reimbursement);
        await _context.SaveChangesAsync(cancellationToken);

        return new ReimbursementDto
        {
            Id = reimbursement.Id,
            EmployeeId = reimbursement.EmployeeId,
            EmployeeName = employee.FullName,
            Amount = reimbursement.Amount,
            Category = reimbursement.Category,
            Description = reimbursement.Description,
            ReceiptUrl = reimbursement.ReceiptUrl,
            Status = reimbursement.Status,
            CreatedAtUtc = reimbursement.CreatedAtUtc,
            CreatedBy = reimbursement.CreatedBy
        };
    }
}

public sealed record ApproveReimbursementCommand(
    Guid ReimbursementId,
    Guid ApprovedByUserId) : IRequest<bool>;

public sealed class ApproveReimbursementCommandHandler : IRequestHandler<ApproveReimbursementCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public ApproveReimbursementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(ApproveReimbursementCommand request, CancellationToken cancellationToken)
    {
        var reimbursement = await _context.Reimbursements
            .FirstOrDefaultAsync(r => r.Id == request.ReimbursementId, cancellationToken);

        if (reimbursement == null)
            return false;

        reimbursement.Approve(request.ApprovedByUserId);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}

public sealed record RejectReimbursementCommand(
    Guid ReimbursementId,
    Guid RejectedByUserId,
    string Reason) : IRequest<bool>;

public sealed class RejectReimbursementCommandHandler : IRequestHandler<RejectReimbursementCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public RejectReimbursementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(RejectReimbursementCommand request, CancellationToken cancellationToken)
    {
        var reimbursement = await _context.Reimbursements
            .FirstOrDefaultAsync(r => r.Id == request.ReimbursementId, cancellationToken);

        if (reimbursement == null)
            return false;

        reimbursement.Reject(request.RejectedByUserId, request.Reason);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
