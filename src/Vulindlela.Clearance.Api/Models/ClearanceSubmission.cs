
namespace Vulindlela.Clearance.Api.Models;

public class ClearanceSubmission
{
    public int ClearanceSubmissionId { get; set; }

    public int AppointmentId { get; set; }

    public string CitizenId { get; set; } = string.Empty;

    public string ServiceType { get; set; } = string.Empty;

    public ClearanceStatus Status { get; set; } =
        ClearanceStatus.Pending;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }

    public string? VerificationMessage { get; set; }

    public List<SubmittedDocument> Documents { get; set; } = new();

    public List<MissingDocument> MissingDocuments { get; set; } = new();
}
