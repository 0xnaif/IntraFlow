using IntraFlow.Domain.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Application.Requests.Commands.CreateRequest;

public sealed record CreateRequestCommand(
    string Title,
    string Description,
    RequestPriority Priority,
    int RequestTypeId);
