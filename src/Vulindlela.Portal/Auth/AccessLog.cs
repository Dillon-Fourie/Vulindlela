
namespace Vulindlela.Portal.Models.Auth;

public class AccessLog
{
    public int Id { get; set; }

    public string CitizenId { get; set; } = string.Empty;

    public string AccessedBy { get; set; } = string.Empty;

    public string AccessedByRole { get; set; } = string.Empty;

    public DateTime AccessedAt { get; set; } = DateTime.UtcNow;

    public string Reason { get; set; } = string.Empty;

    public string ResourceType { get; set; } = string.Empty;

    public string ResourceId { get; set; } = string.Empty;
}
