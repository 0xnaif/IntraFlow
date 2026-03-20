namespace IntraFlow.Application.Requests.DTOs;

using IntraFlow.Domain.Requests;

public sealed record ApproverRequestListItemDto(
    int Id,
    string Title,
    RequestStatus Status,
    RequestPriority Priority,
    DateTime CreatedAt,
    string CreatedByUserId
);