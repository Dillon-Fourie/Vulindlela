
using System.ComponentModel.DataAnnotations;

namespace Vulindlela.Portal.Models;

public class CreateBookingViewModel
{
    [Required]
    [Display(Name = "Appointment Slot ID")]
    [Range(1, int.MaxValue)]
    public int SlotId { get; set; }

    [Required]
    [Display(Name = "Citizen ID")]
    public string CitizenId { get; set; } = string.Empty;

    [Required]
    [Display(Name = "South African ID Number")]
    public string CitizenIdNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Full Name")]
    public string CitizenName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Service Type")]
    public string ServiceType { get; set; } = string.Empty;
}
