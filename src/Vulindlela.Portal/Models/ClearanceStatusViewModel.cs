
namespace Vulindlela.Portal.Models;

public class ClearanceStatusViewModel
{
    public int ClearanceSubmissionId { get; set; }

    public int AppointmentId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? VerificationMessage { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public List<MissingDocumentViewModel> MissingDocuments { get; set; } = new();
}

public class MissingDocumentViewModel
{
    public string DocumentType { get; set; } = string.Empty;
}
