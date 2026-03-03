using System.Security.Claims;
using IntraFlow.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace IntraFlow.Web.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("No authenticated user.");

    public bool IsInRole(string role)
    {
        throw new NotImplementedException();
    }
}