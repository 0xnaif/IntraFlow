using IntraFlow.Application.Requests.DTOs;

namespace IntraFlow.Web.Models.Requests;

public sealed class RequestDetailsPageViewModel
{
    public RequestDetailsDto Request { get; init; } = null!;
    public List<RequestCommentDto> Comments { get; init; } = new();
}