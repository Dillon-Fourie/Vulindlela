
using System.Net;
using System.Net.Http.Json;
using Vulindlela.Portal.Models;

namespace Vulindlela.Portal.Services;

public class AppointmentPortalService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AppointmentPortalService> _logger;

    public AppointmentPortalService(
        IHttpClientFactory httpClientFactory,
        ILogger<AppointmentPortalService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<AppointmentDetailsViewModel?> GetAppointmentDetailsAsync(
        int appointmentId)
    {
        var bookingClient = _httpClientFactory.CreateClient("BookingApi");

        // Booking is the source of truth for appointment details.
        AppointmentViewModel? appointment;

        try
        {
            using var bookingResponse = await bookingClient.GetAsync(
                $"api/appointments/{appointmentId}");

            if (bookingResponse.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            bookingResponse.EnsureSuccessStatusCode();

            appointment = await bookingResponse.Content
                .ReadFromJsonAsync<AppointmentViewModel>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex,
                "Booking API unavailable for appointment {AppointmentId}",
                appointmentId);

            throw;
        }

        if (appointment == null)
        {
            return null;
        }

        var result = new AppointmentDetailsViewModel
        {
            Appointment = appointment,
            IsClearanceAvailable = true,
            HasClearanceSubmission = false,
            ClearanceMessage = "Document submission is still required."
        };

        var clearanceClient = _httpClientFactory.CreateClient("ClearanceApi");

        try
        {
            using var clearanceResponse = await clearanceClient.GetAsync(
                $"api/clearance/appointments/{appointmentId}");

            if (clearanceResponse.StatusCode == HttpStatusCode.NotFound)
            {
                result.ClearanceMessage =
                    "Your appointment is confirmed. " +
                    "No document clearance submission has been received yet.";

                return result;
            }

            clearanceResponse.EnsureSuccessStatusCode();

            result.Clearance = await clearanceResponse.Content
                .ReadFromJsonAsync<ClearanceStatusViewModel>();

            if (result.Clearance != null)
            {
                result.HasClearanceSubmission = true;

                result.ClearanceMessage =
                    result.Clearance.VerificationMessage
                    ?? $"Document clearance status: {result.Clearance.Status}";
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex,
                "Clearance API unavailable for appointment {AppointmentId}",
                appointmentId);

            SetClearanceUnavailable(result);
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex,
                "Clearance API timed out for appointment {AppointmentId}",
                appointmentId);

            SetClearanceUnavailable(result);
        }
        catch (System.Text.Json.JsonException ex)
        {
            _logger.LogWarning(ex,
                "Invalid Clearance response for appointment {AppointmentId}",
                appointmentId);

            SetClearanceUnavailable(result);
        }

        return result;
    }

    private static void SetClearanceUnavailable(
        AppointmentDetailsViewModel result)
    {
        result.IsClearanceAvailable = false;
        result.HasClearanceSubmission = false;
        result.Clearance = null;

        result.ClearanceMessage =
            "Your appointment is confirmed. " +
            "Document clearance information is temporarily unavailable. " +
            "Please check again later.";
    }


    public async Task<List<AppointmentDetailsViewModel>>
        GetCitizenAppointmentsAsync(string citizenId)
    {
        if (string.IsNullOrWhiteSpace(citizenId))
        {
            return new List<AppointmentDetailsViewModel>();
        }

        var bookingClient = _httpClientFactory.CreateClient("BookingApi");

        var appointments = await bookingClient.GetFromJsonAsync<
            List<AppointmentViewModel>>(
                $"api/appointments/citizen/{Uri.EscapeDataString(citizenId.Trim())}");

        var results = new List<AppointmentDetailsViewModel>();

        if (appointments == null)
        {
            return results;
        }

        foreach (var appointment in appointments)
        {
            var details = await GetAppointmentDetailsAsync(
                appointment.AppointmentId);

            if (details != null)
            {
                results.Add(details);
            }
        }

        return results;
    }

    public async Task<BookingSubmissionResult> CreateBookingAsync(
        CreateBookingViewModel booking)
    {
        var bookingClient = _httpClientFactory.CreateClient("BookingApi");

        try
        {
            using var response = await bookingClient.PostAsJsonAsync(
                "api/appointments",
                new
                {
                    booking.SlotId,
                    booking.CitizenId,
                    booking.CitizenIdNumber,
                    booking.CitizenName,
                    booking.ServiceType
                });

            if (response.IsSuccessStatusCode)
            {
                using var document = await System.Text.Json.JsonDocument
                    .ParseAsync(await response.Content.ReadAsStreamAsync());

                var appointmentId = document.RootElement
                    .GetProperty("appointmentId").GetInt32();

                return new BookingSubmissionResult
                {
                    Success = true,
                    AppointmentId = appointmentId,
                    Message = "Your appointment has been booked successfully."
                };
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                return new BookingSubmissionResult
                {
                    Success = false,
                    Message = "This appointment slot is no longer available. " +
                            "Please select another slot."
                };
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return new BookingSubmissionResult
                {
                    Success = false,
                    Message = "The booking request was rejected. " +
                            "Please check your details and selected slot."
                };
            }

            _logger.LogWarning(
                "Booking API returned HTTP {StatusCode} during booking",
                (int)response.StatusCode);

            return new BookingSubmissionResult
            {
                Success = false,
                Message = "Unable to complete your booking right now. " +
                        "Please try again later."
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Booking API unavailable during booking");

            return new BookingSubmissionResult
            {
                Success = false,
                Message = "The booking service is temporarily unavailable. " +
                        "Please try again later."
            };
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Booking API timed out during booking");

            return new BookingSubmissionResult
            {
                Success = false,
                Message = "The booking request timed out. " +
                        "Please check My Appointments before trying again."
            };
        }
    }


}
