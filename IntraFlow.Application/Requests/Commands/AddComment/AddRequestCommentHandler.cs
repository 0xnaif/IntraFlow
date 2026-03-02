using IntraFlow.Application.Abstractions;
using IntraFlow.Application.Common;
using IntraFlow.Domain.Requests;
using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Application.Requests.Commands.AddComment;

public sealed class AddRequestCommentHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AddRequestCommentHandler(
        IAppDbContext db,
        ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(AddRequestCommentCommand cmd, CancellationToken ct = default)
    {
        var request = await _db.Requests
            .FirstOrDefaultAsync(x => x.Id == cmd.RequestId, ct);

        if (request is null)
            throw new InvalidOperationException("Request not found.");

        if (request.Status is RequestStatus.Approved
            or RequestStatus.Rejected
            or RequestStatus.Cancelled)
            throw new InvalidOperationException("Cannot comment on finalized request.");

        if (request.CreatedByUserId != _currentUser.UserId &&
            request.AssignedApproverUserId != _currentUser.UserId)
            throw new UnauthorizedAccessException("Not allowed to comment.");

        if (string.IsNullOrWhiteSpace(cmd.Body))
            throw new ArgumentException("Comment cannot be empty.");

        _db.RequestComments.Add(new RequestComment(
            requestId: cmd.RequestId,
            authorUserId: _currentUser.UserId,
            body: cmd.Body));

        _db.AuditLogs.Add(AuditHelper.Create(
            entityType: "Request",
            entityId: cmd.RequestId.ToString(),
            actionType: "CommentAdded",
            performedByUserId: _currentUser.UserId,
            newValues: new { Comment = cmd.Body }));

        await _db.SaveChangesAsync(ct);
    }
}