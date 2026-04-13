using IntraFlow.Application.Audit.DTOs;
using IntraFlow.Application.Requests.DTOs;

namespace IntraFlow.Web.Models.Requests;

public sealed class RequestDetailsPageViewModel
{
    public RequestDetailsDto Request { get; init; } = null!;
    public List<RequestCommentDto> Comments { get; init; } = new();
    public List<RequestAttachmentDto> Attachments { get; init; } = new();
    public List<AuditEntryDto> AuditEntries { get; init; } = new();

    public bool IsAuthorizedViewer { get; init; }
    public bool IsCreator { get; init; }
    public bool IsAssignedApprover { get; init; }

    public bool CanSubmit { get; init; }
    public bool CanCancel { get; init; }
    public bool CanStartReview { get; init; }
    public bool CanApprove { get; init; }
    public bool CanReject { get; init; }

    public bool CanComment { get; init; }
    public bool CanUploadAttachment { get; init; }
    public bool CanDeleteAttachment { get; init; }

    public bool HasAnyAvailableAction { get; init; }

    public string? SuccessMessage { get; init; }
    public string? ErrorMessage { get; init; }
}