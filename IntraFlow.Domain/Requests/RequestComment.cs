using IntraFlow.Domain.Common;

namespace IntraFlow.Domain.Requests;

public class RequestComment : BaseEntity
{
    public int RequestId { get; private set; }
    public string AuthorUserId { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    private RequestComment() { }

    public RequestComment(int requestId, string authorUserId, string body)
    {
        RequestId = requestId;
        AuthorUserId = authorUserId;
        Body = body;
        CreatedAt = DateTime.UtcNow;
    }
}
