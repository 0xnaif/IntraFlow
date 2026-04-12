using System.Text.Json;

namespace IntraFlow.Application.Audit;

internal static class AuditEntrySummaryBuilder
{
    public static string Build(
        string actionType,
        string? oldValuesJson,
        string? newValuesJson,
        string? notes)
    {
        return actionType switch
        {
            "Created" => BuildCreatedSummary(newValuesJson),
            "Submitted" => BuildStatusTransitionSummary("Request submitted", oldValuesJson, newValuesJson),
            "ReviewStarted" => BuildStatusTransitionSummary("Review started", oldValuesJson, newValuesJson),
            "Approved" => BuildStatusTransitionSummary("Request approved", oldValuesJson, newValuesJson),
            "Rejected" => BuildStatusTransitionSummary("Request rejected", oldValuesJson, newValuesJson),
            "Cancelled" => BuildStatusTransitionSummary("Request cancelled", oldValuesJson, newValuesJson),
            "CommentAdded" => BuildCommentSummary(newValuesJson),
            "AttachmentAdded" => BuildAttachmentAddedSummary(newValuesJson),
            "AttachmentDeleted" => BuildAttachmentDeletedSummary(oldValuesJson),
            _ => BuildFallbackSummary(actionType, notes)
        };
    }

    private static string BuildCreatedSummary(string? newValuesJson)
    {
        var status = ReadString(newValuesJson, "Status");

        return string.IsNullOrWhiteSpace(status)
            ? "Request created."
            : $"Request created with status '{status}'.";
    }

    private static string BuildStatusTransitionSummary(
        string label,
        string? oldValuesJson,
        string? newValuesJson)
    {
        var oldStatus = ReadString(oldValuesJson, "Status");
        var newStatus = ReadString(newValuesJson, "Status");

        if (!string.IsNullOrWhiteSpace(oldStatus) && !string.IsNullOrWhiteSpace(newStatus))
            return $"{label}: {oldStatus} → {newStatus}.";

        return $"{label}.";
    }

    private static string BuildCommentSummary(string? newValuesJson)
    {
        var comment = ReadString(newValuesJson, "Comment");

        if (string.IsNullOrWhiteSpace(comment))
            return "Comment added.";

        comment = comment.Trim();

        if (comment.Length > 100)
            comment = comment[..100] + "...";

        return $"Comment added: \"{comment}\"";
    }

    private static string BuildAttachmentAddedSummary(string? newValuesJson)
    {
        var fileName = ReadString(newValuesJson, "FileName");

        return string.IsNullOrWhiteSpace(fileName)
            ? "Attachment added."
            : $"Attachment added: {fileName}.";
    }

    private static string BuildAttachmentDeletedSummary(string? oldValuesJson)
    {
        var fileName = ReadString(oldValuesJson, "FileName");

        return string.IsNullOrWhiteSpace(fileName)
            ? "Attachment deleted."
            : $"Attachment deleted: {fileName}.";
    }

    private static string BuildFallbackSummary(string actionType, string? notes)
    {
        if (!string.IsNullOrWhiteSpace(notes))
            return $"{actionType}: {notes}";

        return actionType;
    }

    private static string? ReadString(string? json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty(propertyName, out var property))
                return null;

            return property.ValueKind switch
            {
                JsonValueKind.String => property.GetString(),
                JsonValueKind.Number => property.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => property.GetRawText()
            };
        }
        catch
        {
            return null;
        }
    }
}