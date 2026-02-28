using IntraFlow.Domain.Audit;
using System.Text.Json;

namespace IntraFlow.Application.Common;

internal static class AuditHelper
{
    internal static AuditLog Create(
        string entityType,
        string entityId,
        string actionType,
        string performedByUserId,
        object? oldValues = null,
        object? newValues = null,
        string? notes = null)
    {
        return new AuditLog(
            entityType: entityType,
            entityId: entityId,
            actionType: actionType,
            performedByUserId: performedByUserId,
            oldValuesJson: oldValues == null ? null : JsonSerializer.Serialize(oldValues),
            newValuesJson: newValues == null ? null : JsonSerializer.Serialize(newValues),
            notes: notes);
    }
}