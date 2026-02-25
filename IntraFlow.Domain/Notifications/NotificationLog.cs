using IntraFlow.Domain.Common;

namespace IntraFlow.Domain.Notifications;

public class NotificationLog : BaseEntity
{
    public int? RequestId { get; private set; }
    public string EventType { get; private set; } = null!;
    public string RecipientEmail { get; private set; } = null!;
    public string Subject { get; private set; } = null!;
    public string Status { get; private set; } = null!;
    public DateTime? SentAt { get; private set; }
    public string? FailureReason { get; private set; }

    private NotificationLog() { }

    private NotificationLog(
        int? requestId,
        string eventType,
        string recipientEmail,
        string subject,
        string status,
        DateTime? sentAt,
        string? failureReason)
    {
        RequestId = requestId;
        EventType = eventType;
        RecipientEmail = recipientEmail;
        Subject = subject;
        Status = status;
        SentAt = sentAt;
        FailureReason = failureReason;
    }

    public static NotificationLog Sent(int? requestId, string eventType, string recipientEmail, string subject)
    {
        return new(requestId, eventType, recipientEmail, subject, "Sent", DateTime.UtcNow, null);
    }

    public static NotificationLog Failed(int? requestId, string eventType, string recipientEmail, string subject, string failureReason)
    { 
        return new(requestId, eventType, recipientEmail, subject, "Failed", null, failureReason); 
    }
   
}