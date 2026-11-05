
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vulindlela.Clearance.Api.Models;
using Vulindlela.Clearance.Api.Repositories;
using Vulindlela.Clearance.Api.Services;

namespace Vulindlela.Clearance.Api.BackgroundTasks;

public class ClearanceBackgroundWorker : BackgroundService
{
    private readonly IClearanceQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ClearanceBackgroundWorker> _logger;

    public ClearanceBackgroundWorker(
        IClearanceQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<ClearanceBackgroundWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Clearance background worker started.");

        // Recover submissions that were pending when
        // the application previously stopped.
        await RecoverPendingSubmissionsAsync(stoppingToken);

        await foreach (var submissionId in
            _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessSubmissionAsync(
                    submissionId,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to process clearance submission {SubmissionId}",
                    submissionId);
            }
        }
    }

    private async Task RecoverPendingSubmissionsAsync(
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var repository = scope.ServiceProvider
            .GetRequiredService<IClearanceRepository>();

        var pendingSubmissions =
            await repository.GetPendingSubmissionsAsync();

        foreach (var submission in pendingSubmissions)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // A previously interrupted submission must be
            // processed again from the beginning.
            submission.Status = ClearanceStatus.Pending;
            submission.ProcessedAt = null;

            await _queue.EnqueueAsync(
                submission.ClearanceSubmissionId,
                cancellationToken);
        }

        await repository.SaveChangesAsync();

        _logger.LogInformation(
            "Recovered {Count} pending clearance submissions.",
            pendingSubmissions.Count);
    }

    private async Task ProcessSubmissionAsync(
        int submissionId,
        CancellationToken cancellationToken)
    {
        // A fresh scope is created for each queued item.
        using var scope = _scopeFactory.CreateScope();

        var repository = scope.ServiceProvider
            .GetRequiredService<IClearanceRepository>();

        var verificationService = scope.ServiceProvider
            .GetRequiredService<IDocumentVerificationService>();

        var submission =
            await repository.GetSubmissionByIdAsync(submissionId);

        if (submission == null)
        {
            _logger.LogWarning(
                "Clearance submission {SubmissionId} was not found.",
                submissionId);

            return;
        }

        // Prevent duplicate processing of completed submissions.
        if (submission.Status == ClearanceStatus.Approved ||
            submission.Status == ClearanceStatus.MissingDocuments ||
            submission.Status == ClearanceStatus.Rejected)
        {
            return;
        }

        submission.Status = ClearanceStatus.Processing;
        await repository.SaveChangesAsync();

        _logger.LogInformation(
            "Processing clearance submission {SubmissionId}",
            submissionId);

        // Simulate a verification process lasting at least 3 seconds.
        await Task.Delay(
            TimeSpan.FromSeconds(3),
            cancellationToken);

        try
        {
            var submittedTypes = submission.Documents
                .Select(d => d.DocumentType);

            var missingDocuments =
                verificationService.GetMissingDocuments(
                    submission.ServiceType,
                    submittedTypes);

            // Clear previous verification results.
            submission.MissingDocuments.Clear();

            foreach (var documentType in missingDocuments)
            {
                submission.MissingDocuments.Add(
                    new MissingDocument
                    {
                        DocumentType = documentType
                    });
            }

            if (missingDocuments.Count == 0)
            {
                submission.Status = ClearanceStatus.Approved;
                submission.VerificationMessage =
                    "All required documents have been submitted.";
            }
            else
            {
                submission.Status = ClearanceStatus.MissingDocuments;
                submission.VerificationMessage =
                    $"Missing {missingDocuments.Count} required document(s).";
            }
        }
        catch (ArgumentException ex)
        {
            submission.Status = ClearanceStatus.Rejected;
            submission.VerificationMessage = ex.Message;
        }

        submission.ProcessedAt = DateTime.UtcNow;

        await repository.SaveChangesAsync();

        _logger.LogInformation(
            "Clearance submission {SubmissionId} completed with status {Status}",
            submissionId,
            submission.Status);
    }
}
