using IntraFlow.Application.Abstractions;
using IntraFlow.Domain.Audit;
using IntraFlow.Domain.Notifications;
using IntraFlow.Domain.Requests;
using IntraFlow.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace IntraFlow.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser>, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }

    //public dbset<request> requests => set<request>();
    public DbSet<Request> Requests => Set<Request>();
    public DbSet<RequestType> RequestTypes => Set<RequestType>();
    public DbSet<RequestDecision> RequestDecisions => Set<RequestDecision>();
    public DbSet<RequestComment> RequestComments => Set<RequestComment>();
    public DbSet<RequestAttachment> RequestAttachments => Set<RequestAttachment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

}

