using IntraFlow.Domain.Audit;
using IntraFlow.Domain.Notifications;
using IntraFlow.Domain.Requests;
using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Request> Requests { get; }
    DbSet<RequestType> RequestTypes { get; }
    DbSet<RequestDecision> RequestDecisions { get; }
    DbSet<RequestComment> RequestComments { get; }
    DbSet<RequestAttachment> RequestAttachments { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<NotificationLog> NotificationLogs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

}
