using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Application.Requests.Commands.ApproveRequest;

public sealed record ApproveRequestCommand(int RequestId);
