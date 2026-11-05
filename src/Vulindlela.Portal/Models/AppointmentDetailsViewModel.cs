
namespace Vulindlela.Portal.Models;

public class AppointmentDetailsViewModel
{
    // Appointment information retrieved from Booking API.
    public AppointmentViewModel Appointment { get; set; } = new();

    // Document verification information retrieved from Clearance API.
    public ClearanceStatusViewModel? Clearance { get; set; }

    // Indicates whether Clearance API was reachable.
    public bool IsClearanceAvailable { get; set; } = true;

    // Indicates whether the citizen has submitted documents.
    public bool HasClearanceSubmission { get; set; }

    // Friendly message displayed to the citizen.
    public string ClearanceMessage { get; set; } = string.Empty;
}
