using KaizenTracker.Api.Models;

namespace KaizenTracker.Api.Services;

// The status rules, kept free of the database and HTTP so they can be unit tested directly.
//
//   Submitted ──► UnderReview ──► Approved ──► Completed
//       │              │
//       └──────────────┴──► Rejected
//
// Completed and Rejected are terminal.
public static class ProposalWorkflow
{
    private static readonly Dictionary<ProposalStatus, ProposalStatus[]> AllowedTransitions = new()
    {
        [ProposalStatus.Submitted] = [ProposalStatus.UnderReview, ProposalStatus.Rejected],
        [ProposalStatus.UnderReview] = [ProposalStatus.Approved, ProposalStatus.Rejected],
        [ProposalStatus.Approved] = [ProposalStatus.Completed],
        [ProposalStatus.Rejected] = [],
        [ProposalStatus.Completed] = [],
    };

    // The whole table, served to the frontend so it shows only the valid status buttons
    // without duplicating the rules in TypeScript.
    public static IReadOnlyDictionary<ProposalStatus, ProposalStatus[]> Transitions => AllowedTransitions;

    public static IReadOnlyList<ProposalStatus> NextStatuses(ProposalStatus current) =>
        AllowedTransitions[current];

    public static bool CanTransition(ProposalStatus from, ProposalStatus to) =>
        AllowedTransitions[from].Contains(to);
}
