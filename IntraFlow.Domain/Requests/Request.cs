using IntraFlow.Domain.Common;

namespace IntraFlow.Domain.Requests;

public class Request : AuditableEntity
{
    public string Title { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public RequestPriority Priority { get; private set; }
    public RequestStatus Status { get; private set; }

    public int RequestTypeId { get; private set; }
    public string CreatedByUserId { get; private set; } = null!;
    public string? AssignedApproverUserId { get; private set; }

    public DateTime? SubmittedAt { get; private set; }
    public DateTime? InReviewAt { get; private set; }
    public DateTime? DecisionAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    private Request() { } // What is behined the private constroctor in all entities?

    public Request(string title, string description, RequestPriority priority, int requestTypeId, string createdByUserId)
    {
        Title = title;
        Description = description;
        Priority = priority;
        RequestTypeId = requestTypeId;
        CreatedByUserId = createdByUserId;

        Status = RequestStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        LastUpdatedAt = CreatedAt;
    }

    public void Submit()
    {
        if (Status != RequestStatus.Draft)
            throw new InvalidOperationException("Only draft requests can be submitted.");

        Status = RequestStatus.Submitted;
        SubmittedAt = DateTime.UtcNow;
        Touch();
    }

    public void StartReview(string approverUserId)
    {
        if (Status != RequestStatus.Submitted)
            throw new InvalidOperationException("Request must be submitted first.");

        AssignedApproverUserId = approverUserId;
        Status = RequestStatus.InReview;
        InReviewAt = DateTime.UtcNow;
        Touch();
    }

    public void Approve(string approverUserId, DateTime decidedAt)
    {
        if (Status != RequestStatus.InReview)
            throw new InvalidOperationException("Only requests in review can be approved.");

        if (AssignedApproverUserId != approverUserId)
            throw new UnauthorizedAccessException("Only assigned approver can approve.");

        Status = RequestStatus.Approved;
        DecisionAt = decidedAt;
        Touch();
    }

    public void Reject(string approverUserId, DateTime decidedAt)
    {
        if (Status != RequestStatus.InReview)
            throw new InvalidOperationException("Only requests in review can be rejected.");

        if (AssignedApproverUserId != approverUserId)
            throw new UnauthorizedAccessException("Only assigned approver can reject.");

        DecisionAt = decidedAt;
        Status = RequestStatus.Rejected;
        Touch();
    }

    public void Cancel(string actorUserId)
    {

        if (CreatedByUserId != actorUserId)
            throw new UnauthorizedAccessException("Only creator can cancel.");


        if (Status is RequestStatus.Approved or RequestStatus.Rejected)
            throw new InvalidOperationException("Finalized requests cannot be cancelled.");


        Status = RequestStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        Touch();
    }

    private void Touch()
    {
        LastUpdatedAt = DateTime.UtcNow;
    }
}
