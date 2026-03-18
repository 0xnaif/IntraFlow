using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Application.RequestTypes.Commands.CreateRequestType;

public sealed record CreateRequestTypeCommand(
    string Name,
    string Description,
    string DefaultApproverUserId);
