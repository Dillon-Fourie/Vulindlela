
namespace Vulindlela.Booking.Api.Models;

public class Appointment
{
    public int AppointmentId { get; set; }

    public int SlotId { get; set; }

    public string CitizenId { get; set; } = string.Empty;

    public string CitizenIdNumber { get; set; } = string.Empty;

    public string CitizenName { get; set; } = string.Empty;

    public string ServiceType { get; set; } = string.Empty;

    public DateTime BookingDate { get; set; } = DateTime.UtcNow;

    public AppointmentSlot Slot { get; set; } = null!;
}
