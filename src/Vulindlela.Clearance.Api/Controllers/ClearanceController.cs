
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vulindlela.Clearance.Api.BackgroundTasks;
using Vulindlela.Clearance.Api.DTOs;
using Vulindlela.Clearance.Api.Models;
using Vulindlela.Clearance.Api.Repositories;
using Vulindlela.Clearance.Api.Services;

namespace Vulindlela.Clearance.Api.Controllers;

[ApiController]
[Route("api/clearance")]
public class ClearanceController : ControllerBase
{
    private readonly IClearanceRepository _repository;
    private readonly IClearanceQueue _queue;
    private readonly IDocumentVerificationService _verificationService;

    public ClearanceController(
        IClearanceRepository repository,
        IClearanceQueue queue,
        IDocumentVerificationService verificationService)
    {
        _repository = repository;
        _queue = queue;
        _verificationService = verificationService;
    }

    [HttpPost("submissions")]
    public async Task<IActionResult> SubmitDocuments(
        [FromBody] SubmitClearanceRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CitizenId) ||
            string.IsNullOrWhiteSpace(request.ServiceType))
        {
            return BadRequest(
                "CitizenId and ServiceType are required.");
        }

        if (request.Documents == null)
        {
            return BadRequest("Documents must be provided.");
        }

        // Validate the service type before saving the submission.
        try
        {
            _verificationService.GetRequiredDocuments(
                request.ServiceType);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }

        var existingSubmission =
            await _repository.GetSubmissionByAppointmentIdAsync(
                request.AppointmentId);

        if (existingSubmission != null)
        {
            return Conflict(
                "A clearance submission already exists for this appointment.");
        }

        var submission = new ClearanceSubmission
        {
            AppointmentId = request.AppointmentId,
            CitizenId = request.CitizenId.Trim(),
            ServiceType = request.ServiceType.Trim(),
            Status = ClearanceStatus.Pending,
            SubmittedAt = DateTime.UtcNow,
            Documents = request.Documents.Select(d =>
                new SubmittedDocument
                {
                    DocumentType = d.DocumentType.Trim(),
                    FileName = d.FileName.Trim()
                }).ToList()
        };

        try
        {
            await _repository.AddSubmissionAsync(submission);
        }
        catch (DbUpdateException)
        {
            // Also handles a concurrent request attempting
            // to submit documents for the same appointment.
            return Conflict(
                "A clearance submission already exists for this appointment.");
        }

        await _queue.EnqueueAsync(
            submission.ClearanceSubmissionId,
            CancellationToken.None);

        return AcceptedAtAction(
            nameof(GetSubmission),
            new { submissionId = submission.ClearanceSubmissionId },
            new
            {
                submission.ClearanceSubmissionId,
                submission.AppointmentId,
                Status = submission.Status.ToString(),
                Message = "Submission accepted for background verification."
            });
    }

    [HttpGet("submissions/{submissionId:int}")]
    public async Task<IActionResult> GetSubmission(int submissionId)
    {
        var submission =
            await _repository.GetSubmissionByIdAsync(submissionId);

        if (submission == null)
        {
            return NotFound();
        }

        return Ok(ToResponse(submission));
    }

    [HttpGet("appointments/{appointmentId:int}")]
    public async Task<IActionResult> GetByAppointment(int appointmentId)
    {
        var submission =
            await _repository.GetSubmissionByAppointmentIdAsync(
                appointmentId);

        if (submission == null)
        {
            return NotFound();
        }

        return Ok(ToResponse(submission));
    }

    private static object ToResponse(ClearanceSubmission submission)
    {
        return new
        {
            submission.ClearanceSubmissionId,
            submission.AppointmentId,
            submission.CitizenId,
            submission.ServiceType,
            Status = submission.Status.ToString(),
            submission.SubmittedAt,
            submission.ProcessedAt,
            submission.VerificationMessage,
            Documents = submission.Documents.Select(d => new
            {
                d.DocumentType,
                d.FileName
            }),
            MissingDocuments = submission.MissingDocuments
                .Select(d => d.DocumentType)
        };
    }
}
