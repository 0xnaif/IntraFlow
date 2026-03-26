namespace IntraFlow.Application.Requests.DTOs;

public sealed record RequestCommentDto(
    int Id,
    int RequestId,
    string Body,
    string AuthorUserId,
    string AuthorFullName,
    DateTime CreatedAt);