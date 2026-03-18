using IntraFlow.Application.Abstractions;
using IntraFlow.Application.RequestTypes.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Application.RequestTypes.Queries;

public sealed class GetRequestTypesHandler
{
    private readonly IAppDbContext _db;

    public GetRequestTypesHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<List<RequestTypeListItemDto>> Handle(GetRequestTypesQuery query, CancellationToken ct = default)
    {
        return await _db.RequestTypes
            .OrderBy(x => x.Name)
            .Select(x => new RequestTypeListItemDto(
                x.Id,
                x.Name,
                x.Description,
                x.DefaultApproverUserId))
            .ToListAsync(ct);
    }
}
