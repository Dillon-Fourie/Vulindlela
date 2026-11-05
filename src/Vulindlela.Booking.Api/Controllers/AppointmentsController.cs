
using Microsoft.AspNetCore.Mvc;
using Vulindlela.Booking.Api.Services;

namespace Vulindlela.Booking.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public AppointmentsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    // POST: api/appointments
    [HttpPost]
    public async Task<IActionResult> BookAppointment(
        [FromBody] BookingRequest request)
    {
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

    // GET: api/appointments/1
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
}
