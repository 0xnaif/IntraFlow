using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Application.Requests.Commands.CancelRequest;

public sealed record CancelRequestCommand(int RequestId);
