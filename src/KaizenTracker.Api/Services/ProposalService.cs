using KaizenTracker.Api.Data;
using KaizenTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace KaizenTracker.Api.Services;

// Registered as Scoped: it depends on AppDbContext, which is Scoped, so it must not outlive a request.
// TimeProvider is injected instead of calling DateTimeOffset.UtcNow so tests can control the clock.
public class ProposalService(AppDbContext db, TimeProvider clock) : IProposalService
{
    public async Task<List<Proposal>> ListAsync(ProposalStatus? status, CancellationToken ct = default)
    {
        // Builds an IQueryable; nothing hits the database until ToListAsync().
        var query = db.Proposals.AsNoTracking();
        if (status is not null)
        {
            query = query.Where(p => p.Status == status);
        }

        return await query.OrderByDescending(p => p.SubmittedAt).ToListAsync(ct);
    }

    public async Task<Proposal?> GetAsync(int id, CancellationToken ct = default) =>
        await db.Proposals.FindAsync([id], ct);

    public async Task<Proposal> CreateAsync(CreateProposalRequest request, CancellationToken ct = default)
    {
        var proposal = new Proposal
        {
            Title = request.Title,
            Description = request.Description,
            Area = request.Area,
            SubmittedAt = clock.GetUtcNow(),
        };

        db.Proposals.Add(proposal);
        await db.SaveChangesAsync(ct); // INSERT runs here; proposal.Id is filled in afterwards.
        return proposal;
    }

    public async Task<StatusChangeResult> ChangeStatusAsync(int id, ProposalStatus newStatus, CancellationToken ct = default)
    {
        var proposal = await db.Proposals.FindAsync([id], ct); // tracked, so changes are detected
        if (proposal is null)
        {
            return new StatusChangeResult(StatusChangeOutcome.NotFound);
        }

        if (!ProposalWorkflow.CanTransition(proposal.Status, newStatus))
        {
            var allowed = ProposalWorkflow.NextStatuses(proposal.Status);
            var error = allowed.Count == 0
                ? $"{proposal.Status} is a final status and cannot be changed."
                : $"Cannot move from {proposal.Status} to {newStatus}. Allowed: {string.Join(", ", allowed)}.";
            return new StatusChangeResult(StatusChangeOutcome.InvalidTransition, proposal, error);
        }

        proposal.Status = newStatus;
        if (newStatus == ProposalStatus.Completed)
        {
            proposal.CompletedAt = clock.GetUtcNow();
        }

        await db.SaveChangesAsync(ct); // UPDATE only the changed columns
        return new StatusChangeResult(StatusChangeOutcome.Changed, proposal);
    }
}
