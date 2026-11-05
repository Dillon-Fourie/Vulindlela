
using Vulindlela.Clearance.Api.Models;

namespace Vulindlela.Clearance.Api.Repositories;

public interface IClearanceRepository
{
    Task<ClearanceSubmission> AddSubmissionAsync(
        ClearanceSubmission submission);

    Task<ClearanceSubmission?> GetSubmissionByIdAsync(
        int submissionId);

    Task<ClearanceSubmission?> GetSubmissionByAppointmentIdAsync(
        int appointmentId);

    Task<List<ClearanceSubmission>> GetPendingSubmissionsAsync();

    Task SaveChangesAsync();
}
