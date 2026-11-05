
using Microsoft.EntityFrameworkCore;
using Vulindlela.Clearance.Api.Data;
using Vulindlela.Clearance.Api.Models;

namespace Vulindlela.Clearance.Api.Repositories;

public class ClearanceRepository : IClearanceRepository
{
    private readonly ClearanceContext _context;

    public ClearanceRepository(ClearanceContext context)
    {
        _context = context;
    }

    public async Task<ClearanceSubmission> AddSubmissionAsync(
        ClearanceSubmission submission)
    {
        _context.ClearanceSubmissions.Add(submission);

        await _context.SaveChangesAsync();

        return submission;
    }

    public async Task<ClearanceSubmission?> GetSubmissionByIdAsync(
        int submissionId)
    {
        return await _context.ClearanceSubmissions
            .Include(s => s.Documents)
            .Include(s => s.MissingDocuments)
            .FirstOrDefaultAsync(s =>
                s.ClearanceSubmissionId == submissionId);
    }

    public async Task<ClearanceSubmission?>
        GetSubmissionByAppointmentIdAsync(int appointmentId)
    {
        return await _context.ClearanceSubmissions
            .Include(s => s.Documents)
            .Include(s => s.MissingDocuments)
            .FirstOrDefaultAsync(s =>
                s.AppointmentId == appointmentId);
    }

    public async Task<List<ClearanceSubmission>>
        GetPendingSubmissionsAsync()
    {
        return await _context.ClearanceSubmissions
            .Where(s =>
                s.Status == ClearanceStatus.Pending ||
                s.Status == ClearanceStatus.Processing)
            .ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
