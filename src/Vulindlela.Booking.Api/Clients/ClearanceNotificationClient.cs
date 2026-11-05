
using System.Net.Http.Json;

namespace Vulindlela.Booking.Api.Clients;

public class ClearanceNotificationClient
    : IClearanceNotificationClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClearanceNotificationClient> _logger;

    public ClearanceNotificationClient(
        HttpClient httpClient,
        ILogger<ClearanceNotificationClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task NotifyAppointmentCreatedAsync(
        int appointmentId,
        string citizenId,
        string serviceType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var notification = new
            {
                AppointmentId = appointmentId,
                CitizenId = citizenId,
                ServiceType = serviceType
            };

            using var response = await _httpClient.PostAsJsonAsync(
                "api/clearance/appointments",
                notification,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Clearance notification failed for appointment {AppointmentId}. HTTP {StatusCode}",
                    appointmentId,
                    (int)response.StatusCode);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(
                ex,
                "Clearance unavailable for appointment {AppointmentId}. Booking remains successful.",
                appointmentId);
        }
        catch (TaskCanceledException ex)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                ex,
                "Clearance notification timed out for appointment {AppointmentId}.",
                appointmentId);
        }
    }
}
