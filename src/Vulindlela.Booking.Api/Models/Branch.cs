
namespace Vulindlela.Booking.Api.Models;

public class Branch
{
    public int BranchId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string BranchCode { get; set; } = string.Empty;

    public ICollection<AppointmentSlot> Slots { get; set; }
        = new List<AppointmentSlot>();
}
