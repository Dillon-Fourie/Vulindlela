
namespace Vulindlela.Portal.Models;

public class BookingSubmissionResult
{
    public bool Success { get; set; }

    public int? AppointmentId { get; set; }

    public string Message { get; set; } = string.Empty;
}
