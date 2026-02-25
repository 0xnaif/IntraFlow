using IntraFlow.Domain.Common;

namespace IntraFlow.Domain.Requests;

public class RequestType : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public string? DefaultApproverUserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private RequestType() { }

    public RequestType(string name, string? description, string? defaultApproverUserId, bool isActive = true)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
        DefaultApproverUserId = defaultApproverUserId;
        CreatedAt = DateTime.UtcNow;
    }
}
