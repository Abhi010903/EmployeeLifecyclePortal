using EmployeeLifecyclePortal.Application.DTOs.Payroll;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Payroll;

public sealed record GetReimbursementsQuery(Guid? EmployeeId = null, string? Status = null) : IRequest<List<ReimbursementDto>>;

public sealed class GetReimbursementsQueryHandler : IRequestHandler<GetReimbursementsQuery, List<ReimbursementDto>>
{
    private readonly IApplicationDbContext _context;

    public GetReimbursementsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ReimbursementDto>> Handle(GetReimbursementsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Reimbursements
            .Include(r => r.Employee)
            .AsNoTracking();

        if (request.EmployeeId.HasValue && request.EmployeeId.Value != Guid.Empty)
        {
            query = query.Where(r => r.EmployeeId == request.EmployeeId.Value);
        }

        if (!string.IsNullOrEmpty(request.Status) && request.Status != "All")
        {
            query = query.Where(r => r.Status == request.Status);
        }

        var reimbursements = await query
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var approverIds = reimbursements.Where(r => r.ApprovedByUserId.HasValue).Select(r => r.ApprovedByUserId!.Value).Distinct().ToList();
        var approvers = await _context.Employees
            .Where(e => approverIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id, e => e.FullName, cancellationToken);

        return reimbursements.Select(r =>
        {
            string? approverName = null;
            if (r.ApprovedByUserId.HasValue && approvers.TryGetValue(r.ApprovedByUserId.Value, out var name))
            {
                approverName = name;
            }

            return new ReimbursementDto
            {
                Id = r.Id,
                EmployeeId = r.EmployeeId,
                EmployeeName = r.Employee?.FullName ?? "Employee",
                Amount = r.Amount,
                Category = r.Category,
                Description = r.Description,
                ReceiptUrl = r.ReceiptUrl,
                Status = r.Status,
                ApprovedByName = approverName,
                ApprovedAtUtc = r.ApprovedAtUtc,
                RejectionReason = r.RejectionReason,
                PayrollPeriod = r.PayrollPeriod,
                CreatedAtUtc = r.CreatedAtUtc,
                CreatedBy = r.CreatedBy
            };
        }).ToList();
    }
}
