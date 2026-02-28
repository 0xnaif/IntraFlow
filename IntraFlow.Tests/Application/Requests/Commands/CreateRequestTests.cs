using FluentAssertions;
using IntraFlow.Application.Requests.Commands.CreateRequest;
using IntraFlow.Domain.Requests;
using IntraFlow.Tests.Application.Fakes;
using IntraFlow.Tests.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Tests.Application.Requests.Commands;

public class CreateRequestTests
{
    [Fact]
    public async Task Create_creates_audit_log_with_real_entity_id()
    {
        await using var db = DbFactory.Create();

        var currentUser = new FakeCurrentUserService { UserId = "user-creator" };

        db.RequestTypes.Add(new RequestType("Leave Request", "Test", defaultApproverUserId: "approver-1"));
        await db.SaveChangesAsync();

        var handler = new CreateRequestHandler(db, currentUser);

        var requestTypeId = await db.RequestTypes.Select(x => x.Id).FirstAsync();

        var requestId = await handler.Handle(new CreateRequestCommand(
            Title: "Need leave",
            Description: "Personal",
            Priority: RequestPriority.Medium,
            RequestTypeId: requestTypeId
        ));

        var log = await db.AuditLogs.FirstOrDefaultAsync(x =>
            x.EntityType == "Request" &&
            x.ActionType == "Created" &&
            x.EntityId == requestId.ToString());

        log.Should().NotBeNull();
    }
}