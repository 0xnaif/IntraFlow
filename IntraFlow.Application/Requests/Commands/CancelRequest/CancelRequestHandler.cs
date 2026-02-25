using IntraFlow.Application.Abstractions;
using IntraFlow.Domain.Audit;
using IntraFlow.Domain.Requests;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Application.Requests.Commands.CancelRequest;

public sealed class CancelRequestHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CancelRequestHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(CancelRequestCommand cmd, CancellationToken ct = default)
    {
        var request = await _db.Requests
            .FirstOrDefaultAsync(x => x.Id == cmd.RequestId, ct)
            ?? throw new InvalidOperationException("Request not found.");


        request.Cancel(_currentUser.UserId);

        _db.AuditLogs.Add(new AuditLog(
            "Request",
            request.Id.ToString(),  
            "Cancelled",
            _currentUser.UserId,
            null,
            "{ \"Status\": \"Cancelled\" }",
            null));

        await _db.SaveChangesAsync(ct);
    }
}
