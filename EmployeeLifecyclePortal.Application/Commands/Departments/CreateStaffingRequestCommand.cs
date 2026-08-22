using EmployeeLifecyclePortal.Application.DTOs.Departments;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Departments;

public sealed record CreateStaffingRequestCommand(
    Guid DepartmentId,
    int RequiredCount,
    string Reason)
    : IRequest<StaffingRequestDto>;

public sealed class CreateStaffingRequestCommandHandler : IRequestHandler<CreateStaffingRequestCommand, StaffingRequestDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateStaffingRequestCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<StaffingRequestDto> Handle(CreateStaffingRequestCommand request, CancellationToken cancellationToken)
    {
        var department = await _context.Departments
            .FirstOrDefaultAsync(d => d.Id == request.DepartmentId, cancellationToken);

        if (department == null)
            throw new InvalidOperationException("Department not found.");

        var currentHeadcount = await _context.Employees
            .CountAsync(e => e.DepartmentId == request.DepartmentId && e.Status == Domain.Enums.EmploymentStatus.Active, cancellationToken);

        var userIdString = _currentUserService.GetCurrentUserId();
        Guid.TryParse(userIdString, out var userId);

        var staffingRequest = new StaffingRequest(
            department.Id,
            userId,
            currentHeadcount,
            request.RequiredCount,
            request.Reason);

        _context.StaffingRequests.Add(staffingRequest);
        await _context.SaveChangesAsync(cancellationToken);

        var userName = _currentUserService.Email ?? "Manager";

        return new StaffingRequestDto
        {
            Id = staffingRequest.Id,
            DepartmentId = department.Id,
            DepartmentName = department.Name,
            RequestedByUserId = userId,
            RequestedByName = userName,
            CurrentHeadcount = currentHeadcount,
            RequiredCount = staffingRequest.RequiredCount,
            Reason = staffingRequest.Reason,
            Status = staffingRequest.Status,
            CreatedAtUtc = staffingRequest.CreatedAtUtc
        };
    }
}
