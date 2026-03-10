namespace IntraFlow.Application.Requests.DTOs;

using IntraFlow.Domain.Requests;

public sealed record RequestDetailsDto(
    int Id,
    string Title,
    string Description,
    RequestPriority Priority,
    RequestStatus Status,
    string CreatedByUserId,
    DateTime CreatedAt,
    string? AssignedApproverUserId);