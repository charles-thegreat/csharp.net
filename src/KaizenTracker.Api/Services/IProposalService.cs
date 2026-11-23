using KaizenTracker.Api.Models;

namespace KaizenTracker.Api.Services;

public interface IProposalService
{
    Task<List<Proposal>> ListAsync(ProposalStatus? status, CancellationToken ct = default);
    Task<Proposal?> GetAsync(int id, CancellationToken ct = default);
    Task<Proposal> CreateAsync(CreateProposalRequest request, CancellationToken ct = default);
    Task<StatusChangeResult> ChangeStatusAsync(int id, ProposalStatus newStatus, CancellationToken ct = default);
}

public enum StatusChangeOutcome
{
    Changed,
    NotFound,
    InvalidTransition,
}

// Expected failures (unknown id, disallowed transition) are returned as values, not thrown:
// they are normal business outcomes, and the controller maps each one to an HTTP status.
public record StatusChangeResult(StatusChangeOutcome Outcome, Proposal? Proposal = null, string? Error = null);
