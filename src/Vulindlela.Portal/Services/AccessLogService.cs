
using Microsoft.EntityFrameworkCore;
using Vulindlela.Portal.Data;
using Vulindlela.Portal.Models.Auth;

namespace Vulindlela.Portal.Services;

public class AccessLogService
{
    private readonly AuthDbContext _db;

    public AccessLogService(AuthDbContext db)
    {
        _db = db;
    }

    public async Task RecordAccessAsync(
        string citizenId,
        string accessedBy,
        string accessedByRole,
        string reason,
        string resourceType,
        string resourceId)
    {
        var log = new AccessLog
        {
            CitizenId = citizenId,
            AccessedBy = accessedBy,
            AccessedByRole = accessedByRole,
            AccessedAt = DateTime.UtcNow,
            Reason = reason,
            ResourceType = resourceType,
            ResourceId = resourceId
        };

        _db.AccessLogs.Add(log);

        await _db.SaveChangesAsync();
    }

    public async Task<List<AccessLog>> GetCitizenAccessLogsAsync(
        string citizenId)
    {
        return await _db.AccessLogs
            .AsNoTracking()
            .Where(log => log.CitizenId == citizenId)
            .OrderByDescending(log => log.AccessedAt)
            .ToListAsync();
    }
}
