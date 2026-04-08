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
    private readonly IUserLookupService _userLookupService;

    public SubmitRequestHandler(IAppDbContext db, ICurrentUserService currentUser, IEmailSender email, IUserLookupService userLookupService)
    {
        _db = db;
        _currentUser = currentUser;
        _userLookupService = userLookupService;
        _email = email;
    }

    public async Task Handle(SubmitRequestCommand cmd, CancellationToken ct = default)
    {
        var request = await _db.Requests.FirstOrDefaultAsync(x => x.Id == cmd.RequestId, ct)
            ?? throw new InvalidOperationException("Request not found.");

        var requestType = await _db.RequestTypes.FirstOrDefaultAsync(x => x.Id == request.RequestTypeId, ct)
            ?? throw new InvalidOperationException("Request Type not found.");

        if (request.CreatedByUserId != _currentUser.UserId)
            throw new UnauthorizedAccessException("Only the creator can submit the request.");

        var oldStatus = request.Status.ToString();

        request.Submit(requestType.DefaultApproverUserId);

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
        var approverEmail = string.Empty;

        try
        {
            approverEmail = await _userLookupService.RequireEmailByUserIdAsync(request.AssignedApproverUserId!, ct);

            await _email.SendAsync(approverEmail, subject, body, ct);
            _db.NotificationLogs.Add(NotificationLog.Sent(request.Id, "RequestSubmitted", approverEmail, subject));
        }
        catch (Exception ex)
        {
            _db.NotificationLogs.Add(NotificationLog.Failed(request.Id, "RequestSubmitted", string.IsNullOrWhiteSpace(approverEmail) ? "N/A" : approverEmail, subject, ex.Message));
        }

        await _db.SaveChangesAsync(ct);
    }
}
