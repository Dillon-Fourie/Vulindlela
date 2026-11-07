
using Microsoft.EntityFrameworkCore;
using Vulindlela.Booking.Api.Data;
using Vulindlela.Booking.Api.Models;

namespace Vulindlela.Booking.Api.Services;

public class BookingAccessLogService
{
    private readonly BookingContext _context;

    public BookingAccessLogService(BookingContext context)
    {
        _context = context;
    }

    public async Task RecordAccessAsync(
        string citizenId,
        string accessedBy,
        string accessedByRole,
        string reason,
        string resourceType,
        string resourceId)
    {
        var log = new BookingAccessLog
        {
            CitizenId = citizenId,
            AccessedBy = accessedBy,
            AccessedByRole = accessedByRole,
            AccessedAt = DateTime.UtcNow,
            Reason = reason,
            ResourceType = resourceType,
            ResourceId = resourceId
        };

        _context.BookingAccessLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task<List<BookingAccessLog>> GetLogsForCitizenAsync(
        string citizenId)
    {
        return await _context.BookingAccessLogs
            .AsNoTracking()
            .Where(log => log.CitizenId == citizenId)
            .OrderByDescending(log => log.AccessedAt)
            .ToListAsync();
    }
}
