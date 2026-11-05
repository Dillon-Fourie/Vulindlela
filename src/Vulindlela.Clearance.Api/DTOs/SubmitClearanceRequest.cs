
using System.ComponentModel.DataAnnotations;

namespace Vulindlela.Clearance.Api.DTOs;

public class SubmitClearanceRequest
{
    [Range(1, int.MaxValue)]
    public int AppointmentId { get; set; }

    [Required]
    public string CitizenId { get; set; } = string.Empty;

    [Required]
    public string ServiceType { get; set; } = string.Empty;

    public List<SubmittedDocumentRequest> Documents { get; set; } = new();
}

public class SubmittedDocumentRequest
{
    [Required]
    public string DocumentType { get; set; } = string.Empty;

    [Required]
    public string FileName { get; set; } = string.Empty;
}
