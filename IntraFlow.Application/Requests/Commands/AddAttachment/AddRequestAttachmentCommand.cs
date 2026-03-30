namespace IntraFlow.Application.Requests.Commands.AddAttachment;

public sealed record AddRequestAttachmentCommand(
    int RequestId,
    string FileName,
    string ContentType,
    int FileSizeBytes,
    byte[] FileData);