
using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Data;
using Vulindlela.Booking.Api.Models;

namespace Vulindlela.Booking.Api.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly BookingContext _dbContext;

    public BookingRepository(BookingContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Branch>> GetBranchesAsync()
    {
        return await _dbContext.Branches
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<AppointmentSlot>> GetSlotsByBranchAsync(
        int branchId)
    {
        return await _dbContext.AppointmentSlots
            .AsNoTracking()
            .Where(s => s.BranchId == branchId)
            .OrderBy(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<AppointmentSlot?> GetSlotByIdAsync(
        int slotId)
    {
        return await _dbContext.AppointmentSlots
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SlotId == slotId);
    }

    public async Task<bool> IsSlotTakenAsync(int slotId)
    {
        return await _dbContext.Appointments
            .AnyAsync(a => a.SlotId == slotId);
    }

    public async Task<Appointment> AddAppointmentAsync(
        Appointment appointment)
    {
        _dbContext.Appointments.Add(appointment);

        await _dbContext.SaveChangesAsync();

        return appointment;
    }

    public async Task<Appointment?> GetAppointmentByIdAsync(
        int appointmentId)
    {
        return await _dbContext.Appointments
            .AsNoTracking()
            .Include(a => a.Slot)
            .FirstOrDefaultAsync(
                a => a.AppointmentId == appointmentId);
    }

    
    public async Task<List<Appointment>> GetAppointmentsByCitizenIdAsync(
        string citizenId)
    {
        return await _dbContext.Appointments
            .Include(a => a.Slot)
            .Where(a => a.CitizenId == citizenId)
            .OrderByDescending(a => a.Slot.StartTime)
            .ToListAsync();
    }

}
