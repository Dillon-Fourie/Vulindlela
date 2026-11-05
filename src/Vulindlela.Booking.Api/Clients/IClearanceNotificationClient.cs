
namespace Vulindlela.Booking.Api.Clients;

public interface IClearanceNotificationClient
{
    Task NotifyAppointmentCreatedAsync(
        int appointmentId,
        string citizenId,
        string serviceType,
        CancellationToken cancellationToken = default);
}
