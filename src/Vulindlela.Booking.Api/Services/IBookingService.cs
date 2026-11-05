
using Vulindlela.Booking.Api.Models;

namespace Vulindlela.Booking.Api.Services;

public interface IBookingService
{
    Task<List<Branch>> GetBranchesAsync();

    Task<List<AppointmentSlot>> GetAvailableSlotsAsync(
        int branchId);

    Task<BookingResult> BookAppointmentAsync(
        BookingRequest request);

    Task<Appointment?> GetAppointmentAsync(
        int appointmentId);
}

public class BookingRequest
{
    public int SlotId { get; set; }

    public string CitizenId { get; set; } = string.Empty;

    public string CitizenIdNumber { get; set; } = string.Empty;

    public string CitizenName { get; set; } = string.Empty;

    public string ServiceType { get; set; } = string.Empty;
}

public class BookingResult
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public Appointment? Appointment { get; set; }

    public BookingError Error { get; set; } = BookingError.None;
}

public enum BookingError
{
    None,
    InvalidRequest,
    SlotNotFound,
    SlotUnavailable,
    SlotAlreadyBooked
}
