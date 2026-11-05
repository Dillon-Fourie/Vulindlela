
namespace Vulindlela.Clearance.Api.Models;

public class SubmittedDocument
{
    public int SubmittedDocumentId { get; set; }

    public int ClearanceSubmissionId { get; set; }

    public string DocumentType { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public ClearanceSubmission ClearanceSubmission { get; set; } = null!;
}
