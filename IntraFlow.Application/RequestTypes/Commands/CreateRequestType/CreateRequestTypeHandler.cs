using IntraFlow.Application.Abstractions;
using IntraFlow.Domain.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Application.RequestTypes.Commands.CreateRequestType;

public sealed class CreateRequestTypeHandler
{
    private readonly IAppDbContext _db;

    public CreateRequestTypeHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<int> Handle(CreateRequestTypeCommand cmd, CancellationToken ct = default)
    {
        var requestType = new RequestType(
            name: cmd.Name,
            description: cmd.Description,
            defaultApproverUserId: cmd.DefaultApproverUserId);

        _db.RequestTypes.Add(requestType);

        await _db.SaveChangesAsync(ct);

        return requestType.Id;
    }
}
