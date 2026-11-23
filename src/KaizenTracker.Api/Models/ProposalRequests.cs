using System.ComponentModel.DataAnnotations;

namespace KaizenTracker.Api.Models;

// Request bodies (DTOs). [ApiController] validates these attributes automatically
// and returns 400 with details before the action method runs.
public record CreateProposalRequest(
    [Required, StringLength(200)] string Title,
    [Required] string Description,
    [Required, StringLength(100)] string Area);

public record UpdateStatusRequest([Required] ProposalStatus? Status);
