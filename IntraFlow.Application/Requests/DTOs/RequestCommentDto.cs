namespace IntraFlow.Application.Requests.DTOs;

public sealed record RequestCommentDto(
    int Id,
    string AuthorUserId,
    string Body,
    DateTime CreatedAt);