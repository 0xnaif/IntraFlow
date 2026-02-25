using FluentAssertions;
using IntraFlow.Application.Requests.Commands.CreateRequest;
using IntraFlow.Application.Requests.Commands.SubmitRequest;
using IntraFlow.Domain.Notifications;
using IntraFlow.Domain.Requests;
using IntraFlow.Tests.Application.Fakes;
using IntraFlow.Tests.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Tests.Application.Requests.Commands;

public class SubmitRequestTests
{
    [Fact]
    public async Task Submit_creates_notification_log()
    {
        await using var db = DbFactory.Create();

        
        var currentUser = new FakeCurrentUserService { UserId = "user-creator" };
        var email = new FakeEmailSender();

        
        db.RequestTypes.Add(new RequestType("Leave Request", "Test", defaultApproverUserId: "approver-1"));
        await db.SaveChangesAsync();

        
        var createHandler = new CreateRequestHandler(db, currentUser);
        var requestId = await createHandler.Handle(new CreateRequestCommand(
            Title: "Need leave",
            Description: "Personal",
            Priority: RequestPriority.Medium,
            RequestTypeId: db.RequestTypes.Select(x => x.Id).First()
        ));

        
        var submitHandler = new SubmitRequestHandler(db, currentUser, email);

        
        await submitHandler.Handle(new SubmitRequestCommand(requestId));

        
        var log = await db.NotificationLogs
            .FirstOrDefaultAsync(x => x.RequestId == requestId && x.EventType == "RequestSubmitted");

        log.Should().NotBeNull();
        log!.Status.Should().Be("Sent"); // or "Failed" depending on your implementation
        email.Sent.Should().HaveCount(1);
    }
}
