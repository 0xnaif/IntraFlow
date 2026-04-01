using IntraFlow.Application.Abstractions;
using IntraFlow.Application.Common;
using IntraFlow.Domain.Requests;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Application.Requests.Commands.DeleteAttachment;

public sealed class DeleteAttachmentHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public DeleteAttachmentHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(DeleteAttachmentCommand cmd, CancellationToken ct = default)
    {
        var attachment = await _db.RequestAttachments
            .FirstOrDefaultAsync(x => x.Id == cmd.AttachmentId, ct);

        if (attachment is null)
            throw new InvalidOperationException("Attachment not found.");

        var request = await _db.Requests
            .FirstOrDefaultAsync(x => x.Id == attachment.RequestId, ct);

        if (request is null)
            throw new InvalidOperationException("Request not found.");

        if (request.CreatedByUserId != _currentUser.UserId)
            throw new UnauthorizedAccessException("Only the request creator can delete attachments.");

        if (request.Status != RequestStatus.Draft)
            throw new InvalidOperationException("Attachments can only be deleted while the request is in draft status.");

        _db.RequestAttachments.Remove(attachment);

        _db.AuditLogs.Add(AuditHelper.Create(
            entityType: "Request",
            entityId: request.Id.ToString(),
            actionType: "AttachmentDeleted",
            performedByUserId: _currentUser.UserId,
            oldValues: new
            {
                attachment.Id,
                attachment.FileName,
                attachment.ContentType,
                attachment.FileSizeBytes
            }));

        await _db.SaveChangesAsync(ct);

    }
}
