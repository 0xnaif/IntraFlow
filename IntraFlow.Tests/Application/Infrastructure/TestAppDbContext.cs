using IntraFlow.Application.Abstractions;
using IntraFlow.Domain.Audit;
using IntraFlow.Domain.Notifications;
using IntraFlow.Domain.Requests;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Tests.Application.Infrastructure;

public class TestAppDbContext : DbContext, IAppDbContext
{
    public TestAppDbContext(DbContextOptions<TestAppDbContext> options) : base(options) { }

    public DbSet<Request> Requests => Set<Request>();
    public DbSet<RequestType> RequestTypes => Set<RequestType>();
    public DbSet<RequestDecision> RequestDecisions => Set<RequestDecision>();
    public DbSet<RequestComment> RequestComments => Set<RequestComment>();
    public DbSet<RequestAttachment> RequestAttachments => Set<RequestAttachment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
