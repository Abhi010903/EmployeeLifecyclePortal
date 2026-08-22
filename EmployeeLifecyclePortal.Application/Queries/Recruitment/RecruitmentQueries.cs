using EmployeeLifecyclePortal.Application.DTOs.Recruitment;
using EmployeeLifecyclePortal.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLifecyclePortal.Application.Queries.Recruitment;

// Job Postings
public sealed record GetJobPostingsQuery(string? Status = null) : IRequest<List<JobPostingDto>>;

public sealed class GetJobPostingsQueryHandler : IRequestHandler<GetJobPostingsQuery, List<JobPostingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetJobPostingsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<JobPostingDto>> Handle(GetJobPostingsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.JobPostings.Include(j => j.Department).AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(j => j.Status == request.Status);
        }

        var postings = await query
            .OrderByDescending(j => j.PostedDateUtc)
            .ToListAsync(cancellationToken);

        return postings.Select(j => new JobPostingDto
        {
            Id = j.Id,
            Title = j.Title,
            Description = j.Description,
            DepartmentId = j.DepartmentId,
            DepartmentName = j.Department?.Name ?? "General",
            Status = j.Status,
            PostedDateUtc = j.PostedDateUtc,
            ClosedDateUtc = j.ClosedDateUtc,
            CreatedAtUtc = j.CreatedAtUtc,
            CreatedBy = j.CreatedBy,
            LastModifiedAtUtc = j.LastModifiedAtUtc,
            LastModifiedBy = j.LastModifiedBy
        }).ToList();
    }
}

public sealed record GetJobPostingByIdQuery(Guid Id) : IRequest<JobPostingDto?>;

public sealed class GetJobPostingByIdQueryHandler : IRequestHandler<GetJobPostingByIdQuery, JobPostingDto?>
{
    private readonly IApplicationDbContext _context;

    public GetJobPostingByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<JobPostingDto?> Handle(GetJobPostingByIdQuery request, CancellationToken cancellationToken)
    {
        var j = await _context.JobPostings
            .Include(x => x.Department)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (j == null) return null;

        return new JobPostingDto
        {
            Id = j.Id,
            Title = j.Title,
            Description = j.Description,
            DepartmentId = j.DepartmentId,
            DepartmentName = j.Department?.Name ?? "General",
            Status = j.Status,
            PostedDateUtc = j.PostedDateUtc,
            ClosedDateUtc = j.ClosedDateUtc,
            CreatedAtUtc = j.CreatedAtUtc,
            CreatedBy = j.CreatedBy,
            LastModifiedAtUtc = j.LastModifiedAtUtc,
            LastModifiedBy = j.LastModifiedBy
        };
    }
}

// Candidates
public sealed record GetCandidatesQuery(Guid? JobPostingId = null) : IRequest<List<CandidateDto>>;

public sealed class GetCandidatesQueryHandler : IRequestHandler<GetCandidatesQuery, List<CandidateDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCandidatesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CandidateDto>> Handle(GetCandidatesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Candidates.Include(c => c.JobPosting).AsQueryable();

        if (request.JobPostingId.HasValue && request.JobPostingId.Value != Guid.Empty)
        {
            query = query.Where(c => c.JobPostingId == request.JobPostingId.Value);
        }

        var candidates = await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return candidates.Select(c => new CandidateDto
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email,
            PhoneNumber = c.PhoneNumber,
            Status = c.Status,
            JobPostingId = c.JobPostingId,
            JobPostingTitle = c.JobPosting?.Title ?? "General",
            ResumePath = c.ResumePath,
            CreatedAtUtc = c.CreatedAtUtc,
            CreatedBy = c.CreatedBy,
            LastModifiedAtUtc = c.LastModifiedAtUtc,
            LastModifiedBy = c.LastModifiedBy
        }).ToList();
    }
}

public sealed record GetCandidateByIdQuery(Guid Id) : IRequest<CandidateDto?>;

public sealed class GetCandidateByIdQueryHandler : IRequestHandler<GetCandidateByIdQuery, CandidateDto?>
{
    private readonly IApplicationDbContext _context;

    public GetCandidateByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CandidateDto?> Handle(GetCandidateByIdQuery request, CancellationToken cancellationToken)
    {
        var c = await _context.Candidates
            .Include(x => x.JobPosting)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (c == null) return null;

        return new CandidateDto
        {
            Id = c.Id,
            FirstName = c.FirstName,
            LastName = c.LastName,
            Email = c.Email,
            PhoneNumber = c.PhoneNumber,
            Status = c.Status,
            JobPostingId = c.JobPostingId,
            JobPostingTitle = c.JobPosting?.Title ?? "General",
            ResumePath = c.ResumePath,
            CreatedAtUtc = c.CreatedAtUtc,
            CreatedBy = c.CreatedBy,
            LastModifiedAtUtc = c.LastModifiedAtUtc,
            LastModifiedBy = c.LastModifiedBy
        };
    }
}

// Interviews
public sealed record GetInterviewsQuery(Guid? CandidateId = null) : IRequest<List<InterviewDto>>;

public sealed class GetInterviewsQueryHandler : IRequestHandler<GetInterviewsQuery, List<InterviewDto>>
{
    private readonly IApplicationDbContext _context;

    public GetInterviewsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<InterviewDto>> Handle(GetInterviewsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Interviews.Include(i => i.Candidate).AsQueryable();

        if (request.CandidateId.HasValue && request.CandidateId.Value != Guid.Empty)
        {
            query = query.Where(i => i.CandidateId == request.CandidateId.Value);
        }

        var interviews = await query
            .OrderByDescending(i => i.ScheduledDateUtc)
            .ToListAsync(cancellationToken);

        return interviews.Select(i => new InterviewDto
        {
            Id = i.Id,
            CandidateId = i.CandidateId,
            CandidateName = i.Candidate != null ? $"{i.Candidate.FirstName} {i.Candidate.LastName}".Trim() : "Unknown",
            ScheduledDateUtc = i.ScheduledDateUtc,
            InterviewerName = i.InterviewerName,
            Status = i.Status,
            Rating = i.Rating,
            Feedback = i.Feedback,
            CreatedAtUtc = i.CreatedAtUtc,
            CreatedBy = i.CreatedBy,
            LastModifiedAtUtc = i.LastModifiedAtUtc,
            LastModifiedBy = i.LastModifiedBy
        }).ToList();
    }
}

// Offers
public sealed record GetJobOffersQuery(Guid? CandidateId = null) : IRequest<List<JobOfferDto>>;

public sealed class GetJobOffersQueryHandler : IRequestHandler<GetJobOffersQuery, List<JobOfferDto>>
{
    private readonly IApplicationDbContext _context;

    public GetJobOffersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<JobOfferDto>> Handle(GetJobOffersQuery request, CancellationToken cancellationToken)
    {
        var query = _context.JobOffers.Include(o => o.Candidate).AsQueryable();

        if (request.CandidateId.HasValue && request.CandidateId.Value != Guid.Empty)
        {
            query = query.Where(o => o.CandidateId == request.CandidateId.Value);
        }

        var offers = await query
            .OrderByDescending(o => o.OfferDateUtc)
            .ToListAsync(cancellationToken);

        return offers.Select(o => new JobOfferDto
        {
            Id = o.Id,
            CandidateId = o.CandidateId,
            CandidateName = o.Candidate != null ? $"{o.Candidate.FirstName} {o.Candidate.LastName}".Trim() : "Unknown",
            OfferedSalary = o.OfferedSalary,
            OfferDateUtc = o.OfferDateUtc,
            ExpiryDateUtc = o.ExpiryDateUtc,
            Status = o.Status,
            CreatedAtUtc = o.CreatedAtUtc,
            CreatedBy = o.CreatedBy,
            LastModifiedAtUtc = o.LastModifiedAtUtc,
            LastModifiedBy = o.LastModifiedBy
        }).ToList();
    }
}
