namespace IntraFlow.Application.Requests.DTOs;

public sealed record RequestAttachmentDto(
    int Id,
    string FileName,
    int FileSizeBytes,
    DateTime UploadedAt);