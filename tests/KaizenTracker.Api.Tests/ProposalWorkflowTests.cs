using KaizenTracker.Api.Models;
using KaizenTracker.Api.Services;

namespace KaizenTracker.Api.Tests;

// Pure unit tests: no database, no HTTP, no DI. ProposalWorkflow is a static table of rules.
public class ProposalWorkflowTests
{
    // [Theory] + [InlineData] = one test method run once per data row (like pytest.mark.parametrize).
    [Theory]
    [InlineData(ProposalStatus.Submitted)]
    [InlineData(ProposalStatus.UnderReview)]
    [InlineData(ProposalStatus.Rejected)]
    public void Cannot_complete_a_proposal_that_was_not_approved(ProposalStatus from)
    {
        Assert.False(ProposalWorkflow.CanTransition(from, ProposalStatus.Completed));
    }

    [Fact]
    public void Can_complete_an_approved_proposal()
    {
        Assert.True(ProposalWorkflow.CanTransition(ProposalStatus.Approved, ProposalStatus.Completed));
    }

    [Theory]
    [InlineData(ProposalStatus.Submitted, ProposalStatus.UnderReview)]
    [InlineData(ProposalStatus.Submitted, ProposalStatus.Rejected)]
    [InlineData(ProposalStatus.UnderReview, ProposalStatus.Approved)]
    [InlineData(ProposalStatus.UnderReview, ProposalStatus.Rejected)]
    public void Allows_the_normal_review_path(ProposalStatus from, ProposalStatus to)
    {
        Assert.True(ProposalWorkflow.CanTransition(from, to));
    }

    [Theory]
    [InlineData(ProposalStatus.Completed)]
    [InlineData(ProposalStatus.Rejected)]
    public void Final_statuses_allow_no_further_changes(ProposalStatus final)
    {
        Assert.Empty(ProposalWorkflow.NextStatuses(final));
    }

    // Guards against adding a new enum value and forgetting to give it rules:
    // NextStatuses would throw KeyNotFoundException for a status missing from the table.
    [Fact]
    public void Every_status_has_transition_rules()
    {
        foreach (var status in Enum.GetValues<ProposalStatus>())
        {
            var exception = Record.Exception(() => ProposalWorkflow.NextStatuses(status));
            Assert.Null(exception);
        }
    }
}
