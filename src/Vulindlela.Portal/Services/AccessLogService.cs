
using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Vulindlela.Portal.Data;
using Vulindlela.Portal.Models.Auth;

namespace Vulindlela.Portal.Services;

public class AccessLogService
{
    private readonly AuthDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;

    public AccessLogService(
        AuthDbContext db,
        IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
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
        var portalLogs = await _db.AccessLogs
            .AsNoTracking()
            .Where(log => log.CitizenId == citizenId)
            .ToListAsync();

        var bookingClient = _httpClientFactory.CreateClient("BookingApi");

        using var response = await bookingClient.GetAsync(
            $"api/accesslogs/citizen/{Uri.EscapeDataString(citizenId)}");

        if (response.StatusCode == HttpStatusCode.Forbidden ||
            response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new UnauthorizedAccessException(
                "You are not authorised to view these access logs.");
        }

        response.EnsureSuccessStatusCode();

        var bookingLogs = await response.Content
            .ReadFromJsonAsync<List<BookingAccessLogDto>>()
            ?? new List<BookingAccessLogDto>();

        var combinedLogs = portalLogs.Concat(
            bookingLogs.Select(log => new AccessLog
            {
                CitizenId = log.CitizenId,
                AccessedBy = log.AccessedBy,
                AccessedByRole = log.AccessedByRole,
                AccessedAt = log.AccessedAt,
                Reason = log.Reason,
                ResourceType = log.ResourceType,
                ResourceId = log.ResourceId
            }));

        return combinedLogs
            .OrderByDescending(log => log.AccessedAt)
            .ToList();
    }

    private sealed class BookingAccessLogDto
    {
        public string CitizenId { get; set; } = string.Empty;
        public string AccessedBy { get; set; } = string.Empty;
        public string AccessedByRole { get; set; } = string.Empty;
        public DateTime AccessedAt { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string ResourceType { get; set; } = string.Empty;
        public string ResourceId { get; set; } = string.Empty;
    }
}
