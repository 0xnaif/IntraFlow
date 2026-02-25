using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Application.Requests.Commands.RejectRequest;

public sealed record RejectRequestCommand(int RequestId, string Reason);
