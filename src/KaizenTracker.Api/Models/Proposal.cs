namespace KaizenTracker.Api.Models;

public enum ProposalStatus
{
    Submitted,
    UnderReview,
    Approved,
    Rejected,
    Completed,
}

public class Proposal
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Area { get; set; }
    public ProposalStatus Status { get; set; } = ProposalStatus.Submitted;
    public DateTimeOffset SubmittedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
