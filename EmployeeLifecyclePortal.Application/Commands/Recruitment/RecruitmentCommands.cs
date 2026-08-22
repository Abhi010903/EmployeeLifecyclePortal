using EmployeeLifecyclePortal.Application.DTOs.Recruitment;
using EmployeeLifecyclePortal.Application.Interfaces;
using EmployeeLifecyclePortal.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Commands.Recruitment;

// Job Postings
public sealed record CreateJobPostingCommand(
    string Title,
    string Description,
    Guid DepartmentId) : IRequest<JobPostingDto>;

public sealed class CreateJobPostingCommandHandler : IRequestHandler<CreateJobPostingCommand, JobPostingDto>
{
    private readonly IApplicationDbContext _context;

    public CreateJobPostingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<JobPostingDto> Handle(CreateJobPostingCommand request, CancellationToken cancellationToken)
    {
        var posting = new JobPosting(request.Title, request.Description, request.DepartmentId);
        _context.JobPostings.Add(posting);
        await _context.SaveChangesAsync(cancellationToken);

        var department = await _context.Departments.FirstOrDefaultAsync(d => d.Id == request.DepartmentId, cancellationToken);

        return new JobPostingDto
        {
            Id = posting.Id,
            Title = posting.Title,
            Description = posting.Description,
            DepartmentId = posting.DepartmentId,
            DepartmentName = department?.Name ?? "General",
            Status = posting.Status,
            PostedDateUtc = posting.PostedDateUtc,
            CreatedAtUtc = posting.CreatedAtUtc,
            CreatedBy = posting.CreatedBy
        };
    }
}

public sealed record CloseJobPostingCommand(Guid Id) : IRequest;

public sealed class CloseJobPostingCommandHandler : IRequestHandler<CloseJobPostingCommand>
{
    private readonly IApplicationDbContext _context;

    public CloseJobPostingCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(CloseJobPostingCommand request, CancellationToken cancellationToken)
    {
        var posting = await _context.JobPostings.FirstOrDefaultAsync(j => j.Id == request.Id, cancellationToken);
        if (posting == null)
            throw new InvalidOperationException("Job posting not found.");

        posting.Close();
        await _context.SaveChangesAsync(cancellationToken);
    }
}

// Candidates
public sealed record CreateCandidateCommand(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    Guid JobPostingId) : IRequest<CandidateDto>;

public sealed class CreateCandidateCommandHandler : IRequestHandler<CreateCandidateCommand, CandidateDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCandidateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CandidateDto> Handle(CreateCandidateCommand request, CancellationToken cancellationToken)
    {
        var candidate = new Candidate(request.FirstName, request.LastName, request.Email, request.JobPostingId);
        if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
        {
            candidate.SetPhoneNumber(request.PhoneNumber);
        }

        _context.Candidates.Add(candidate);
        await _context.SaveChangesAsync(cancellationToken);

        var jobPosting = await _context.JobPostings.FirstOrDefaultAsync(j => j.Id == request.JobPostingId, cancellationToken);

        return new CandidateDto
        {
            Id = candidate.Id,
            FirstName = candidate.FirstName,
            LastName = candidate.LastName,
            Email = candidate.Email,
            PhoneNumber = candidate.PhoneNumber,
            Status = candidate.Status,
            JobPostingId = candidate.JobPostingId,
            JobPostingTitle = jobPosting?.Title ?? "General",
            CreatedAtUtc = candidate.CreatedAtUtc,
            CreatedBy = candidate.CreatedBy
        };
    }
}

public sealed record UpdateCandidateStatusCommand(Guid Id, string Status) : IRequest;

public sealed class UpdateCandidateStatusCommandHandler : IRequestHandler<UpdateCandidateStatusCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateCandidateStatusCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateCandidateStatusCommand request, CancellationToken cancellationToken)
    {
        var candidate = await _context.Candidates.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);
        if (candidate == null)
            throw new InvalidOperationException("Candidate not found.");

        candidate.UpdateStatus(request.Status);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

// Interviews
public sealed record ScheduleInterviewCommand(
    Guid CandidateId,
    DateTime ScheduledDateUtc,
    string InterviewerName) : IRequest<InterviewDto>;

public sealed class ScheduleInterviewCommandHandler : IRequestHandler<ScheduleInterviewCommand, InterviewDto>
{
    private readonly IApplicationDbContext _context;

    public ScheduleInterviewCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InterviewDto> Handle(ScheduleInterviewCommand request, CancellationToken cancellationToken)
    {
        var candidate = await _context.Candidates.FirstOrDefaultAsync(c => c.Id == request.CandidateId, cancellationToken);
        if (candidate == null)
            throw new InvalidOperationException("Candidate not found.");

        var interview = new Interview(request.CandidateId, request.ScheduledDateUtc, request.InterviewerName);
        _context.Interviews.Add(interview);

        candidate.MoveToInterview();
        await _context.SaveChangesAsync(cancellationToken);

        return new InterviewDto
        {
            Id = interview.Id,
            CandidateId = interview.CandidateId,
            CandidateName = candidate.FullName,
            ScheduledDateUtc = interview.ScheduledDateUtc,
            InterviewerName = interview.InterviewerName,
            Status = interview.Status,
            CreatedAtUtc = interview.CreatedAtUtc,
            CreatedBy = interview.CreatedBy
        };
    }
}

public sealed record CompleteInterviewCommand(
    Guid Id,
    int Rating,
    string Feedback) : IRequest;

public sealed class CompleteInterviewCommandHandler : IRequestHandler<CompleteInterviewCommand>
{
    private readonly IApplicationDbContext _context;

    public CompleteInterviewCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(CompleteInterviewCommand request, CancellationToken cancellationToken)
    {
        var interview = await _context.Interviews.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        if (interview == null)
            throw new InvalidOperationException("Interview not found.");

        interview.Complete(request.Rating, request.Feedback);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

// Offers
public sealed record CreateJobOfferCommand(
    Guid CandidateId,
    decimal OfferedSalary,
    DateTime ExpiryDateUtc) : IRequest<JobOfferDto>;

public sealed class CreateJobOfferCommandHandler : IRequestHandler<CreateJobOfferCommand, JobOfferDto>
{
    private readonly IApplicationDbContext _context;

    public CreateJobOfferCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<JobOfferDto> Handle(CreateJobOfferCommand request, CancellationToken cancellationToken)
    {
        var candidate = await _context.Candidates.FirstOrDefaultAsync(c => c.Id == request.CandidateId, cancellationToken);
        if (candidate == null)
            throw new InvalidOperationException("Candidate not found.");

        var offer = new JobOffer(request.CandidateId, request.OfferedSalary, request.ExpiryDateUtc);
        _context.JobOffers.Add(offer);
        await _context.SaveChangesAsync(cancellationToken);

        return new JobOfferDto
        {
            Id = offer.Id,
            CandidateId = offer.CandidateId,
            CandidateName = candidate.FullName,
            OfferedSalary = offer.OfferedSalary,
            OfferDateUtc = offer.OfferDateUtc,
            ExpiryDateUtc = offer.ExpiryDateUtc,
            Status = offer.Status,
            CreatedAtUtc = offer.CreatedAtUtc,
            CreatedBy = offer.CreatedBy
        };
    }
}

public sealed record AcceptJobOfferCommand(Guid Id) : IRequest;

public sealed class AcceptJobOfferCommandHandler : IRequestHandler<AcceptJobOfferCommand>
{
    private readonly IApplicationDbContext _context;

    public AcceptJobOfferCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AcceptJobOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _context.JobOffers.Include(o => o.Candidate).FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (offer == null)
            throw new InvalidOperationException("Job offer not found.");

        offer.Accept();
        if (offer.Candidate != null)
        {
            offer.Candidate.UpdateStatus("Hired");
        }
        await _context.SaveChangesAsync(cancellationToken);
    }
}

public sealed record RejectJobOfferCommand(Guid Id) : IRequest;

public sealed class RejectJobOfferCommandHandler : IRequestHandler<RejectJobOfferCommand>
{
    private readonly IApplicationDbContext _context;

    public RejectJobOfferCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RejectJobOfferCommand request, CancellationToken cancellationToken)
    {
        var offer = await _context.JobOffers.Include(o => o.Candidate).FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (offer == null)
            throw new InvalidOperationException("Job offer not found.");

        offer.Reject();
        if (offer.Candidate != null)
        {
            offer.Candidate.UpdateStatus("Rejected");
        }
        await _context.SaveChangesAsync(cancellationToken);
    }
}
