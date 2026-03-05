namespace IntraFlow.Application.Requests.DTOs;

public sealed record MyRequestListItemDto(
    int Id,
    string Title,
    string Status,
    string Priority,
    DateTime CreatedAt);