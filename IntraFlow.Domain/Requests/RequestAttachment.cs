using IntraFlow.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Domain.Requests;

public class RequestAttachment : AuditableEntity
{
    public int RequestId { get; private set; }
    public string FileName { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public int FileSizeBytes { get; private set; }
    public byte[] FileData { get; private set; } = null!;
    public string UploadedByUserId { get; private set; } = null!;
    public DateTime UploadedAt { get; private set; }

    private RequestAttachment() { }
    public RequestAttachment(
        int requestId,
        string fileName,
        string contentType,
        int fileSizeBytes,
        byte[] fileData,
        string uploadedByUserId)
    {
        RequestId = requestId;
        FileName = fileName;
        ContentType = contentType;
        FileSizeBytes = fileSizeBytes;
        FileData = fileData;
        UploadedByUserId = uploadedByUserId;
        UploadedAt = DateTime.UtcNow;
    }
}
