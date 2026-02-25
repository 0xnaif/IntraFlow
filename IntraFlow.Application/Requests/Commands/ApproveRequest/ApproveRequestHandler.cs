using IntraFlow.Application.Abstractions;
using IntraFlow.Domain.Audit;
using IntraFlow.Domain.Notifications;
using IntraFlow.Domain.Requests;
using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Application.Requests.Commands.ApproveRequest;

public sealed class ApproveRequestHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailSender _email;

    public ApproveRequestHandler(IAppDbContext db, ICurrentUserService currentUser, IEmailSender email)
    {
        _db = db;
        _currentUser = currentUser;
        _email = email;
    }

    public async Task Handle(ApproveRequestCommand cmd, CancellationToken ct = default)
    {
        var request = await _db.Requests
            .FirstOrDefaultAsync(x => x.Id == cmd.RequestId, ct)
            ?? throw new InvalidOperationException("Request not found.");

        if (request.Status != RequestStatus.InReview)
            throw new InvalidOperationException("Only requests in review can be approved.");

        if (request.AssignedApproverUserId != _currentUser.UserId)
            throw new UnauthorizedAccessException("Only assigned approver can approve.");

        var decision = RequestDecision.Approve(request.Id, _currentUser.UserId);

        request.Approve(_currentUser.UserId, decision.DecidedAt);

        _db.RequestDecisions.Add(decision);

        _db.AuditLogs.Add(new AuditLog(
            entityType: "Request",
            entityId: request.Id.ToString(),
            actionType: "Approved",
            performedByUserId: _currentUser.UserId,
            oldValuesJson: "{ \"Status\": \"InReview\" }",
            newValuesJson: "{ \"Status\": \"Approved\" }",
            notes: null));

        var subject = $"Request #{request.Id} approved";
        var body = $"Request '{request.Title}' has been approved.";

        try
        {
            await _email.SendAsync("admin@test.com", subject, body, ct);
            _db.NotificationLogs.Add(NotificationLog.Sent(request.Id, "RequestApproved", "admin@test.com", subject));
        }
        catch (Exception ex)
        {
            _db.NotificationLogs.Add(NotificationLog.Failed(request.Id, "RequestApproved", "admin@test.com", subject, ex.Message));
        }

        await _db.SaveChangesAsync(ct);
    }
}
