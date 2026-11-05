
namespace Vulindlela.Clearance.Api.Models;

public class ClearanceAppointment
{
    public int AppointmentId { get; set; }

    public string CitizenId { get; set; } = string.Empty;

    public string ServiceType { get; set; } = string.Empty;

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}
