
namespace Vulindlela.Booking.Api.Models;

public class AppointmentSlot
{
    public int SlotId { get; set; }

    public int BranchId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsBookable { get; set; } = true;

    public Branch Branch { get; set; } = null!;

    public Appointment? Appointment { get; set; }
}
