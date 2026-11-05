
using System.ComponentModel.DataAnnotations;

namespace Vulindlela.Clearance.Api.DTOs;

public class RegisterAppointmentRequest
{
    [Range(1, int.MaxValue)]
    public int AppointmentId { get; set; }

    [Required]
    public string CitizenId { get; set; } = string.Empty;

    [Required]
    public string ServiceType { get; set; } = string.Empty;
}
