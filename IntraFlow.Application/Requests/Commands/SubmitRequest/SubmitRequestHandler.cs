using IntraFlow.Application.Abstractions;
using IntraFlow.Application.Common;
using IntraFlow.Domain.Audit;
using IntraFlow.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace IntraFlow.Application.Requests.Commands.SubmitRequest;

public sealed class SubmitRequestHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailSender _email;

    public SubmitRequestHandler(IAppDbContext db, ICurrentUserService currentUser, IEmailSender email)
    {
        _db = db;
        _currentUser = currentUser;
        _email = email;
    }

    public async Task Handle(SubmitRequestCommand cmd, CancellationToken ct = default)
    {
        var request = await _db.Requests.FirstOrDefaultAsync(x => x.Id == cmd.RequestId, ct)
            ?? throw new InvalidOperationException("Request not found.");

        if (request.CreatedByUserId != _currentUser.UserId)
            throw new UnauthorizedAccessException("Only the creator can submit the request.");

        var oldStatus = request.Status.ToString();

        request.Submit();

        var newStatus = request.Status.ToString();

        
        _db.AuditLogs.Add(AuditHelper.Create(
            entityType: "Request",
            entityId: request.Id.ToString(),
            actionType: "Submitted",
            performedByUserId: _currentUser.UserId,
            oldValues: new { Status = oldStatus },
            newValues: new { Status = newStatus }));


        var subject = $"Request #{request.Id} submitted";
        var body = $"Request '{request.Title}' has been submitted.";

        try
        {
            await _email.SendAsync("admin@test.com", subject, body, ct);
            _db.NotificationLogs.Add(NotificationLog.Sent(request.Id, "RequestSubmitted", "admin@test.com", subject));
        }
        catch (Exception ex)
        {
            _db.NotificationLogs.Add(NotificationLog.Failed(request.Id, "RequestSubmitted", "admin@test.com", subject, ex.Message));
        }

        await _db.SaveChangesAsync(ct);
    }
}
