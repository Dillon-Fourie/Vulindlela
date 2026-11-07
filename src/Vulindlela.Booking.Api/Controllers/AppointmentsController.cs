
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Vulindlela.Booking.Api.Services;

namespace Vulindlela.Booking.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
private readonly IBookingService _bookingService;
private readonly BookingAccessLogService _accessLogService;

public AppointmentsController(
    IBookingService bookingService,
    BookingAccessLogService accessLogService)
{
    _bookingService = bookingService;
    _accessLogService = accessLogService;
}

    [HttpPost]
    public async Task<IActionResult> BookAppointment(
        [FromBody] BookingRequest request)
    {
        if (!User.IsInRole("Official"))
        {
            if (!User.IsInRole("Citizen"))
            {
                return Forbid();
            }

            var signedInCitizenId = User.FindFirstValue("CitizenId");

            if (string.IsNullOrWhiteSpace(signedInCitizenId))
            {
                return Forbid();
            }

            if (!string.Equals(
                request.CitizenId,
                signedInCitizenId,
                StringComparison.Ordinal))
            {
                return Forbid();
            }
        }

        var result = await _bookingService
            .BookAppointmentAsync(request);

        if (!result.Success)
        {
            return result.Error switch
            {
                BookingError.InvalidRequest =>
                    BadRequest(new { result.Message }),

                BookingError.SlotNotFound =>
                    NotFound(new { result.Message }),

                BookingError.SlotUnavailable =>
                    Conflict(new { result.Message }),

                BookingError.SlotAlreadyBooked =>
                    Conflict(new { result.Message }),

                _ => BadRequest(new { result.Message })
            };
        }

        var appointment = result.Appointment!;

        return CreatedAtAction(
            nameof(GetAppointment),
            new { appointmentId = appointment.AppointmentId },
            new
            {
                appointment.AppointmentId,
                appointment.SlotId,
                appointment.CitizenId,
                appointment.CitizenName,
                appointment.ServiceType,
                appointment.BookingDate
            });
    }

    [HttpGet("{appointmentId:int}")]
    public async Task<IActionResult> GetAppointment(
        int appointmentId)
    {
        var appointment = await _bookingService
            .GetAppointmentAsync(appointmentId);

        if (appointment is null)
        {
            return NotFound(new
            {
                Message = "Appointment not found."
            });
        }

        if (!CanAccessCitizen(appointment.CitizenId))
        {
            return Forbid();
        }

        await _accessLogService.RecordAccessAsync(
            appointment.CitizenId,
            User.Identity?.Name ?? "Unknown",
            User.IsInRole("Official") ? "Official" : "Citizen",
            "Viewed appointment details",
            "Appointment",
            appointment.AppointmentId.ToString());

        return Ok(new
        {
            appointment.AppointmentId,
            appointment.SlotId,
            appointment.CitizenId,
            appointment.CitizenName,
            appointment.ServiceType,
            appointment.BookingDate,
            Slot = new
            {
                appointment.Slot.StartTime,
                appointment.Slot.EndTime,
                appointment.Slot.BranchId
            }
        });
    }

    [HttpGet("citizen/{citizenId}")]
    public async Task<IActionResult> GetCitizenAppointments(
        string citizenId)
    {
        if (string.IsNullOrWhiteSpace(citizenId))
        {
            return BadRequest("A valid citizen ID is required.");
        }

        if (!CanAccessCitizen(citizenId))
        {
            return Forbid();
        }

        var appointments = await _bookingService
            .GetAppointmentsByCitizenIdAsync(citizenId);

        await _accessLogService.RecordAccessAsync(
                citizenId,
                User.Identity?.Name ?? "Unknown",
                User.IsInRole("Official") ? "Official" : "Citizen",
                "Viewed citizen appointments",
                "AppointmentList",
                citizenId);

        return Ok(appointments.Select(a => new
        {
            a.AppointmentId,
            a.SlotId,
            a.CitizenId,
            a.CitizenName,
            a.ServiceType,
            a.BookingDate,
            Slot = a.Slot == null ? null : new
            {
                a.Slot.BranchId,
                a.Slot.StartTime,
                a.Slot.EndTime
            }
        }));
    }

    private bool CanAccessCitizen(string citizenId)
    {
        if (User.IsInRole("Official"))
        {
            return true;
        }

        if (!User.IsInRole("Citizen"))
        {
            return false;
        }

        var signedInCitizenId = User.FindFirstValue("CitizenId");

        return !string.IsNullOrWhiteSpace(signedInCitizenId)
            && string.Equals(
                signedInCitizenId,
                citizenId,
                StringComparison.Ordinal);
    }
}
