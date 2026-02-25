

namespace IntraFlow.Application.Abstractions;

public interface ICurrentUserService
{
    string UserId { get; }
    bool IsInRole(string role);
}
