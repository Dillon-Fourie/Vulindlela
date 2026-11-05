
namespace Vulindlela.Clearance.Api.Models;

public class MissingDocument
{
    public int MissingDocumentId { get; set; }

    public int ClearanceSubmissionId { get; set; }

    public string DocumentType { get; set; } = string.Empty;

    public ClearanceSubmission ClearanceSubmission { get; set; } = null!;
}
