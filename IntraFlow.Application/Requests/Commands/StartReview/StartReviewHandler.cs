using IntraFlow.Application.Abstractions;
using IntraFlow.Application.Common;
using IntraFlow.Domain.Notifications;
using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Application.Requests.Commands.StartReview;

public sealed class StartReviewHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailSender _email;
    private readonly IUserLookupService _userLookupService;

    public StartReviewHandler(IAppDbContext db, ICurrentUserService currentUser, IEmailSender email, IUserLookupService userLookupService)
    {
        _db = db;
        _currentUser = currentUser;
        _email = email;
        _userLookupService = userLookupService;
    }

    public async Task Handle(StartReviewCommand cmd, CancellationToken ct = default)
    {
        var request = await _db.Requests.FirstOrDefaultAsync(x => x.Id == cmd.RequestId, ct)
            ?? throw new InvalidOperationException("Request not found.");

        if (request.AssignedApproverUserId != _currentUser.UserId)
            throw new UnauthorizedAccessException("Only assigned approver can start review.");

        var oldStatus = request.Status;

        request.StartReview(request.AssignedApproverUserId);

        var newStatus = request.Status;

        _db.AuditLogs.Add(AuditHelper.Create(
            entityType: "Request",
            entityId: request.Id.ToString(),
            actionType: "InReview",
            performedByUserId: _currentUser.UserId,
            oldValues: new { Status = oldStatus },
            newValues: new { Status = newStatus }));

        var subject = $"Request #{request.Id} in review";
        var body = $"Request '{request.Title}' has been in review";
        var creatorEmail = string.Empty;

        try
        {
            creatorEmail = await _userLookupService.RequireEmailByUserIdAsync(request.CreatedByUserId, ct);
            await _email.SendAsync(creatorEmail, subject, body, ct);
            _db.NotificationLogs.Add(NotificationLog.Sent(request.Id, "RequestReviewStarted", creatorEmail, subject));
        }
        catch (Exception ex)
        {
            _db.NotificationLogs.Add(NotificationLog.Failed(request.Id, "RequestReviewStarted", creatorEmail, subject, ex.Message));
        }

        await _db.SaveChangesAsync(ct);
    }
}
