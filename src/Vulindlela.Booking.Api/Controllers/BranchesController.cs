
using Microsoft.AspNetCore.Mvc;
using Vulindlela.Booking.Api.Services;

namespace Vulindlela.Booking.Api.Controllers;

[ApiController]
[Route("api/branches")]
public class BranchesController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BranchesController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    // GET: api/branches
    [HttpGet]
    public async Task<IActionResult> GetBranches()
    {
        var branches = await _bookingService.GetBranchesAsync();

        return Ok(branches.Select(b => new
        {
            b.BranchId,
            b.Name,
            b.Address,
            b.BranchCode
        }));
    }

    // GET: api/branches/1/slots
    [HttpGet("{branchId:int}/slots")]
    public async Task<IActionResult> GetAvailableSlots(int branchId)
    {
        if (branchId <= 0)
        {
            return BadRequest("A valid branch ID is required.");
        }

        var slots = await _bookingService
            .GetAvailableSlotsAsync(branchId);

        return Ok(slots.Select(s => new
        {
            s.SlotId,
            s.BranchId,
            s.StartTime,
            s.EndTime
        }));
    }
}
