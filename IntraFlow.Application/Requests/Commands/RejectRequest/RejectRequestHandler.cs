using IntraFlow.Application.Abstractions;
using IntraFlow.Application.Common;
using IntraFlow.Domain.Audit;
using IntraFlow.Domain.Notifications;
using IntraFlow.Domain.Requests;
using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Application.Requests.Commands.RejectRequest;

public sealed class RejectRequestHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailSender _email;

    public RejectRequestHandler(IAppDbContext db, ICurrentUserService currentUser, IEmailSender email)
    {
        _db = db;
        _currentUser = currentUser;
        _email = email;
    }

    public async Task Handle(RejectRequestCommand cmd, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cmd.Reason))
            throw new InvalidOperationException("Reject reason is required.");

        var request = await _db.Requests
            .FirstOrDefaultAsync(x => x.Id == cmd.RequestId, ct)
            ?? throw new InvalidOperationException("Request not found.");

        if (request.Status != RequestStatus.InReview)
            throw new InvalidOperationException("Only requests in review can be rejected.");

        if (request.AssignedApproverUserId != _currentUser.UserId)
            throw new UnauthorizedAccessException("Only assigned approver can reject.");

        var decision = RequestDecision.Reject(
            request.Id,
            cmd.Reason,
            _currentUser.UserId);

        var oldStatus = request.Status.ToString();

        request.Reject(_currentUser.UserId, decision.DecidedAt);

        var newStatus = request.Status.ToString();

        _db.RequestDecisions.Add(decision);

        _db.AuditLogs.Add(AuditHelper.Create(
            entityType: "Request",
            entityId: request.Id.ToString(),
            actionType: "Rejected",
            performedByUserId: _currentUser.UserId,
            oldValues: new { Status = oldStatus },
            newValues: new { Status = newStatus }));

        var subject = $"Request #{request.Id} rejected";
        var body = $"Request '{request.Title}' has been rejected.";

        try
        {
            await _email.SendAsync("admin@test.com", subject, body, ct);
            _db.NotificationLogs.Add(NotificationLog.Sent(request.Id, "RequestRejected", "admin@test.com", subject));
        }
        catch (Exception ex)
        {
            _db.NotificationLogs.Add(NotificationLog.Failed(request.Id, "RequestRejected", "admin@test.com", subject, ex.Message));
        }

        await _db.SaveChangesAsync(ct);
    }
}
