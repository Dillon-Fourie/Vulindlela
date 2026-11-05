
using Vulindlela.Booking.Api.Clients;

namespace Vulindlela.Booking.Tests;

public class FakeClearanceNotificationClient
    : IClearanceNotificationClient
{
    public bool ShouldThrow { get; set; }

    public int NotificationCount { get; private set; }

    public int? LastAppointmentId { get; private set; }

    public Task NotifyAppointmentCreatedAsync(
        int appointmentId,
        string citizenId,
        string serviceType,
        CancellationToken cancellationToken = default)
    {
        NotificationCount++;
        LastAppointmentId = appointmentId;

        if (ShouldThrow)
        {
            throw new HttpRequestException(
                "Simulated Clearance service outage.");
        }

        return Task.CompletedTask;
    }
}
