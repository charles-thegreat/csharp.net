using KaizenTracker.Api.Models;
using KaizenTracker.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace KaizenTracker.Api.Controllers;

// Thin controller: HTTP in, HTTP out. Business rules live in IProposalService.
// The CancellationToken parameters are bound automatically to the request's abort signal,
// so a disconnected client cancels the database query.
[ApiController]
[Route("api/proposals")]
public class ProposalsController(IProposalService proposals) : ControllerBase
{
    // GET api/proposals?status=Approved
    [HttpGet]
    public async Task<ActionResult<List<Proposal>>> List([FromQuery] ProposalStatus? status, CancellationToken ct) =>
        await proposals.ListAsync(status, ct);

    // GET api/proposals/5
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Proposal>> Get(int id, CancellationToken ct)
    {
        var proposal = await proposals.GetAsync(id, ct);
        return proposal is null ? NotFound() : proposal;
    }

    // GET api/proposals/workflow -> { "Submitted": ["UnderReview", "Rejected"], ... }
    [HttpGet("workflow")]
    public ActionResult<IReadOnlyDictionary<ProposalStatus, ProposalStatus[]>> Workflow() =>
        Ok(ProposalWorkflow.Transitions);

    // POST api/proposals
    [HttpPost]
    public async Task<ActionResult<Proposal>> Create(CreateProposalRequest request, CancellationToken ct)
    {
        var proposal = await proposals.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = proposal.Id }, proposal);
    }

    // PATCH api/proposals/5/status
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<Proposal>> UpdateStatus(int id, UpdateStatusRequest request, CancellationToken ct)
    {
        var result = await proposals.ChangeStatusAsync(id, request.Status!.Value, ct);

        return result.Outcome switch
        {
            StatusChangeOutcome.Changed => result.Proposal!,
            StatusChangeOutcome.NotFound => NotFound(),
            // 409 Conflict: the request is well-formed but clashes with the proposal's current state.
            _ => Problem(statusCode: StatusCodes.Status409Conflict, title: "Invalid status change", detail: result.Error),
        };
    }
}
