using KaizenTracker.Api.Data;
using KaizenTracker.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KaizenTracker.Api.Controllers;

[ApiController]
[Route("api/proposals")]
// Primary constructor: DI supplies the request-scoped AppDbContext.
public class ProposalsController(AppDbContext db) : ControllerBase
{
    // GET api/proposals?status=Approved
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Proposal>>> List([FromQuery] ProposalStatus? status)
    {
        // Builds an IQueryable; nothing hits the database until ToListAsync().
        var query = db.Proposals.AsNoTracking();
        if (status is not null)
        {
            query = query.Where(p => p.Status == status);
        }

        return await query.OrderByDescending(p => p.SubmittedAt).ToListAsync();
    }

    // GET api/proposals/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Proposal>> Get(int id)
    {
        var proposal = await db.Proposals.FindAsync(id);
        return proposal is null ? NotFound() : proposal;
    }

    // POST api/proposals
    [HttpPost]
    public async Task<ActionResult<Proposal>> Create(CreateProposalRequest request)
    {
        var proposal = new Proposal
        {
            Title = request.Title,
            Description = request.Description,
            Area = request.Area,
            SubmittedAt = DateTimeOffset.UtcNow,
        };

        db.Proposals.Add(proposal);
        await db.SaveChangesAsync(); // INSERT runs here; proposal.Id is filled in afterwards.

        return CreatedAtAction(nameof(Get), new { id = proposal.Id }, proposal);
    }

    // PATCH api/proposals/5/status
    // Step 3 moves the transition rules (e.g. no Completed before Approved) into a service.
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<Proposal>> UpdateStatus(int id, UpdateStatusRequest request)
    {
        var proposal = await db.Proposals.FindAsync(id); // tracked, so changes are detected
        if (proposal is null) return NotFound();

        proposal.Status = request.Status!.Value;
        proposal.CompletedAt = proposal.Status == ProposalStatus.Completed ? DateTimeOffset.UtcNow : null;

        await db.SaveChangesAsync(); // UPDATE only the changed columns
        return proposal;
    }
}
