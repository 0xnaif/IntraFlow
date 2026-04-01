using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Application.Requests.Commands.DeleteAttachment;

public sealed record DeleteAttachmentCommand(int AttachmentId);
