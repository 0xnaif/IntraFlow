namespace IntraFlow.Application.Requests.Queries.GetComments;

public sealed record RequestCommentDto(
    int Id,
    string AuthorUserId,
    string Body,
    DateTime CreatedAt);