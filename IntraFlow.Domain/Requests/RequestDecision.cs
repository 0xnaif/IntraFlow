using IntraFlow.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Domain.Requests;

public class RequestDecision : BaseEntity
{
    public int RequestId { get; private set; }
    public RequestStatus Decision { get; private set; }
    public string? DecisionReason { get; private set; }
    public string DecidedByUserId { get; private set; } = null!;
    public DateTime DecidedAt { get; private set; }

    private RequestDecision() { }

    private RequestDecision(int requestId, RequestStatus decision, string? reason, string approverUserId)
    {
        RequestId = requestId;
        Decision = decision;
        DecisionReason = reason;
        DecidedByUserId = approverUserId;
        DecidedAt = DateTime.UtcNow;
    }

    public static RequestDecision Approve(int requestId, string approverUserId) 
        => new(requestId, RequestStatus.Approved, null, approverUserId);

    public static RequestDecision Reject(int requestId, string reason, string approverUserId)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new InvalidOperationException("Reject reason is required.");

        return new RequestDecision(requestId, RequestStatus.Rejected, reason, approverUserId);
    }
}
