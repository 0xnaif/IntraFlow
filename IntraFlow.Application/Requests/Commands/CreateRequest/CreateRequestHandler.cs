using IntraFlow.Application.Abstractions;
using IntraFlow.Application.Common;
using IntraFlow.Domain.Requests;
using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Application.Requests.Commands.CreateRequest;

public sealed class CreateRequestHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateRequestHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(CreateRequestCommand cmd, CancellationToken ct = default)
    {
        var typeExists = await _db.RequestTypes
            .AnyAsync(x => x.Id == cmd.RequestTypeId && x.IsActive, ct);

        if (!typeExists)
            throw new InvalidOperationException("Invalid request type.");

        var request = new Request(
            cmd.Title,
            cmd.Description,
            cmd.Priority,
            cmd.RequestTypeId,
            _currentUser.UserId);

        _db.Requests.Add(request);

        var newStatus = request.Status.ToString();

        await _db.SaveChangesAsync(ct);

        _db.AuditLogs.Add(AuditHelper.Create(
            entityType: "Request",
            entityId: request.Id.ToString(),
            actionType: "Created",
            performedByUserId: _currentUser.UserId,
            oldValues: null,
            newValues: new { Status = newStatus }));

        await _db.SaveChangesAsync(ct);

        return request.Id;
    }
}
