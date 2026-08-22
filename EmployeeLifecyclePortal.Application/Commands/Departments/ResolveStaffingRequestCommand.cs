using EmployeeLifecyclePortal.Application.DTOs.Departments;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Departments;

public sealed record ResolveStaffingRequestCommand(
    Guid Id,
    bool Approve,
    string? Comments = null)
    : IRequest<StaffingRequestDto>;

public sealed class ResolveStaffingRequestCommandHandler : IRequestHandler<ResolveStaffingRequestCommand, StaffingRequestDto>
{
    private readonly IApplicationDbContext _context;

    public ResolveStaffingRequestCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<StaffingRequestDto> Handle(ResolveStaffingRequestCommand request, CancellationToken cancellationToken)
    {
        var staffingRequest = await _context.StaffingRequests
            .Include(sr => sr.Department)
            .FirstOrDefaultAsync(sr => sr.Id == request.Id, cancellationToken);

        if (staffingRequest == null)
            throw new InvalidOperationException("Staffing request not found.");

        if (request.Approve)
        {
            staffingRequest.Approve(request.Comments);
        }
        else
        {
            staffingRequest.Reject(request.Comments);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new StaffingRequestDto
        {
            Id = staffingRequest.Id,
            DepartmentId = staffingRequest.DepartmentId,
            DepartmentName = staffingRequest.Department?.Name ?? "Unknown",
            RequestedByUserId = staffingRequest.RequestedByUserId,
            RequestedByName = "Manager",
            CurrentHeadcount = staffingRequest.CurrentHeadcount,
            RequiredCount = staffingRequest.RequiredCount,
            Reason = staffingRequest.Reason,
            Status = staffingRequest.Status,
            AdminComments = staffingRequest.AdminComments,
            CreatedAtUtc = staffingRequest.CreatedAtUtc,
            ResolvedAtUtc = staffingRequest.ResolvedAtUtc
        };
    }
}
