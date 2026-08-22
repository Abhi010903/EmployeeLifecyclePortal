using EmployeeLifecyclePortal.Application.Authorization;
using EmployeeLifecyclePortal.Application.Commands.Recruitment;
using EmployeeLifecyclePortal.Application.Queries.Recruitment;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeLifecyclePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = Permissions.Employee)]
public sealed class RecruitmentController : ControllerBase
{
    private readonly IMediator _mediator;

    public RecruitmentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Job Postings
    [HttpGet("job-postings")]
    public async Task<IActionResult> GetJobPostings(
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetJobPostingsQuery(status),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("job-postings/{id:guid}")]
    public async Task<IActionResult> GetJobPostingById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetJobPostingByIdQuery(id),
            cancellationToken);

        if (result == null)
            return NotFound("Job posting not found.");

        return Ok(result);
    }

    [HttpPost("job-postings")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> CreateJobPosting(
        [FromBody] CreateJobPostingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateJobPostingCommand(request.Title, request.Description, request.DepartmentId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("job-postings/{id:guid}/close")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> CloseJobPosting(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new CloseJobPostingCommand(id),
            cancellationToken);

        return Ok();
    }

    // Candidates
    [HttpGet("candidates")]
    public async Task<IActionResult> GetCandidates(
        [FromQuery] Guid? jobPostingId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetCandidatesQuery(jobPostingId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("candidates/{id:guid}")]
    public async Task<IActionResult> GetCandidateById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetCandidateByIdQuery(id),
            cancellationToken);

        if (result == null)
            return NotFound("Candidate not found.");

        return Ok(result);
    }

    [HttpPost("candidates")]
    public async Task<IActionResult> CreateCandidate(
        [FromBody] CreateCandidateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateCandidateCommand(request.FirstName, request.LastName, request.Email, request.PhoneNumber, request.JobPostingId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("candidates/{id:guid}/status")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> UpdateCandidateStatus(
        Guid id,
        [FromBody] UpdateCandidateStatusRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateCandidateStatusCommand(id, request.Status),
            cancellationToken);

        return Ok();
    }

    // Interviews
    [HttpGet("interviews")]
    public async Task<IActionResult> GetInterviews(
        [FromQuery] Guid? candidateId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetInterviewsQuery(candidateId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("interviews")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> ScheduleInterview(
        [FromBody] ScheduleInterviewRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ScheduleInterviewCommand(request.CandidateId, request.ScheduledDateUtc, request.InterviewerName),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("interviews/{id:guid}/complete")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> CompleteInterview(
        Guid id,
        [FromBody] CompleteInterviewRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new CompleteInterviewCommand(id, request.Rating, request.Feedback),
            cancellationToken);

        return Ok();
    }

    // Offers
    [HttpGet("offers")]
    public async Task<IActionResult> GetJobOffers(
        [FromQuery] Guid? candidateId,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetJobOffersQuery(candidateId),
            cancellationToken);

        return Ok(result);
    }

    [HttpPost("offers")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> CreateJobOffer(
        [FromBody] CreateJobOfferRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateJobOfferCommand(request.CandidateId, request.OfferedSalary, request.ExpiryDateUtc),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("offers/{id:guid}/accept")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> AcceptJobOffer(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new AcceptJobOfferCommand(id),
            cancellationToken);

        return Ok();
    }

    [HttpPut("offers/{id:guid}/reject")]
    [Authorize(Policy = Permissions.Manager)]
    public async Task<IActionResult> RejectJobOffer(
        Guid id,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new RejectJobOfferCommand(id),
            cancellationToken);

        return Ok();
    }
}

public sealed class CreateJobPostingRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
}

public sealed class CreateCandidateRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public Guid JobPostingId { get; set; }
}

public sealed class UpdateCandidateStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public sealed class ScheduleInterviewRequest
{
    public Guid CandidateId { get; set; }
    public DateTime ScheduledDateUtc { get; set; }
    public string InterviewerName { get; set; } = string.Empty;
}

public sealed class CompleteInterviewRequest
{
    public int Rating { get; set; }
    public string Feedback { get; set; } = string.Empty;
}

public sealed class CreateJobOfferRequest
{
    public Guid CandidateId { get; set; }
    public decimal OfferedSalary { get; set; }
    public DateTime ExpiryDateUtc { get; set; }
}
