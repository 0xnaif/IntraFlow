using IntraFlow.Application.Abstractions;
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

        _db.AuditLogs.Add(new(
            entityType: "Request",
            entityId: request.Id.ToString(),
            actionType: "Created",
            performedByUserId: _currentUser.UserId,
            oldValuesJson: null,
            newValuesJson: null,
            notes: null));

        await _db.SaveChangesAsync(ct);
        return request.Id;
    }
}
