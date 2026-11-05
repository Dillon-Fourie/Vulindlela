
namespace Vulindlela.Portal.Models;

public class AppointmentViewModel
{
    public int AppointmentId { get; set; }

    public int SlotId { get; set; }

    public string CitizenId { get; set; } = string.Empty;

    public string CitizenName { get; set; } = string.Empty;

    public string ServiceType { get; set; } = string.Empty;

    public DateTime BookingDate { get; set; }

    public AppointmentSlotViewModel? Slot { get; set; }
}

public class AppointmentSlotViewModel
{
    public int BranchId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }
}
