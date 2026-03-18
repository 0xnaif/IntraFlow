using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Application.RequestTypes.DTOs;

public sealed record RequestTypeListItemDto(
    int Id,
    string Name,
    string Description,
    string? DefaultApproverUserId);
