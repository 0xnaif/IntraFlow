using IntraFlow.Application.Abstractions;
using IntraFlow.Application.Common;
using IntraFlow.Domain.Requests;
using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Application.Requests.Commands.AddAttachment;

public sealed class AddRequestAttachmentHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public AddRequestAttachmentHandler(
        IAppDbContext db,
        ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(AddRequestAttachmentCommand cmd, CancellationToken ct = default)
    {
        var request = await _db.Requests
            .FirstOrDefaultAsync(x => x.Id == cmd.RequestId, ct);

        if (request is null)
            throw new InvalidOperationException("Request not found.");

        if (request.Status is RequestStatus.Approved
            or RequestStatus.Rejected
            or RequestStatus.Cancelled)
            throw new InvalidOperationException("Cannot upload attachments to finalized request.");

        if (request.CreatedByUserId != _currentUser.UserId)
            throw new UnauthorizedAccessException("Not allowed to upload attachments.");

        if (string.IsNullOrWhiteSpace(cmd.FileName))
            throw new ArgumentException("File name is required.");

        if (cmd.FileSizeBytes <= 0 || cmd.FileData.Length == 0)
            throw new ArgumentException("Attachment cannot be empty.");

        var attachment = new RequestAttachment(
            requestId: cmd.RequestId,
            fileName: cmd.FileName,
            contentType: cmd.ContentType,
            fileSizeBytes: cmd.FileSizeBytes,
            fileData: cmd.FileData,
            uploadedByUserId: _currentUser.UserId);

        _db.RequestAttachments.Add(attachment);

        _db.AuditLogs.Add(AuditHelper.Create(
            entityType: "Request",
            entityId: cmd.RequestId.ToString(),
            actionType: "AttachmentAdded",
            performedByUserId: _currentUser.UserId,
            newValues: new
            {
                cmd.FileName,
                cmd.FileSizeBytes,
                cmd.ContentType
            }));

        await _db.SaveChangesAsync(ct);
    }
}