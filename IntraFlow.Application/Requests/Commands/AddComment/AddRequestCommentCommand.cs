namespace IntraFlow.Application.Requests.Commands.AddComment;

public sealed record AddRequestCommentCommand(
    int RequestId,
    string Body);