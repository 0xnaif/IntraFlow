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
        var userLookup = new FakeUserLookupService();

        userLookup.SetUser("approver-1", "Approver One", "approver1@test.com");

        db.RequestTypes.Add(new RequestType("Leave Request", "Test", defaultApproverUserId: "approver-1"));
        await db.SaveChangesAsync();

        
        var createHandler = new CreateRequestHandler(db, currentUser);
        var requestId = await createHandler.Handle(new CreateRequestCommand(
            Title: "Need leave",
            Description: "Personal",
            Priority: RequestPriority.Medium,
            RequestTypeId: db.RequestTypes.Select(x => x.Id).First()
        ));

        
        var submitHandler = new SubmitRequestHandler(db, currentUser, email, userLookup);

        
        await submitHandler.Handle(new SubmitRequestCommand(requestId));

        var audit = await db.AuditLogs.FirstOrDefaultAsync(x =>
            x.EntityType == "Request" &&
            x.EntityId == requestId.ToString() &&
            x.ActionType == "Submitted");


        var log = await db.NotificationLogs
            .FirstOrDefaultAsync(x => x.RequestId == requestId && x.EventType == "RequestSubmitted");

        audit.Should().NotBeNull();
        audit!.OldValuesJson.Should().Contain("Draft");
        audit.NewValuesJson.Should().Contain("Submitted");


        log.Should().NotBeNull();
        log!.Status.Should().Be("Sent"); // or "Failed" depending on your implementation
        email.Sent.Should().HaveCount(1);
    }

    [Fact]
    public async Task Submit_sends_email_to_assigned_approver_and_creates_sent_notification_log()
    {
        await using var db = DbFactory.Create();

        var currentUser = new FakeCurrentUserService { UserId = "user-creator" };
        var email = new FakeEmailSender();
        var userLookup = new FakeUserLookupService();

        userLookup.SetUser("approver-1", "Approver One", "approver1@test.com");

        db.RequestTypes.Add(new RequestType("Leave Request", "Test", defaultApproverUserId: "approver-1"));
        await db.SaveChangesAsync();

        var createHandler = new CreateRequestHandler(db, currentUser);
        var requestId = await createHandler.Handle(new CreateRequestCommand(
            Title: "Need Leave",
            Description: "Personal",
            Priority: RequestPriority.Medium,
            RequestTypeId: db.RequestTypes.Select(x => x.Id).First()
            ));

        var submitHandler = new SubmitRequestHandler(db, currentUser, email, userLookup);

        await submitHandler.Handle(new SubmitRequestCommand(requestId));

        var audit = await db.AuditLogs.FirstOrDefaultAsync(x =>
        x.EntityType == "Request" &&
        x.EntityId == requestId.ToString() &&
        x.ActionType == "Submitted");

        var log = await db.NotificationLogs.FirstOrDefaultAsync(x => x.RequestId == requestId && x.EventType == "RequestSubmitted");

        audit.Should().NotBeNull();
        audit!.OldValuesJson.Should().Contain("Draft");
        audit.NewValuesJson.Should().Contain("Submitted");

        log.Should().NotBeNull();
        log!.Status.Should().Be("Sent");
        log.RecipientEmail.Should().Be("approver1@test.com");

        email.Sent.Should().HaveCount(1);
        email.Sent[0].To.Should().Be("approver1@test.com");
    }

    [Fact]
    public async Task Submit_when_email_send_fails_logs_failure_with_resolved_recipient_email()
    {
        await using var db = DbFactory.Create();

        var currentUser = new FakeCurrentUserService { UserId = "user-creator" };
        var email = new FakeEmailSender
        {
            ThrowOnSend = true,
            ExceptionMessage = "SMTP failed."
        };
        var userLookup = new FakeUserLookupService();

        userLookup.SetUser("approver-1", "Approver One", "approver1@test.com");

        db.RequestTypes.Add(new RequestType("Leave Request", "Test", defaultApproverUserId: "approver-1"));
        await db.SaveChangesAsync();

        var createHandler = new CreateRequestHandler(db, currentUser);
        var requestId = await createHandler.Handle(new CreateRequestCommand(
            Title: "Need leave",
            Description: "Personal",
            Priority: RequestPriority.Medium,
            RequestTypeId: db.RequestTypes.Select(x => x.Id).First()
        ));

        var submitHandler = new SubmitRequestHandler(db, currentUser, email, userLookup);

        await submitHandler.Handle(new SubmitRequestCommand(requestId));

        var log = await db.NotificationLogs
            .FirstOrDefaultAsync(x => x.RequestId == requestId && x.EventType == "RequestSubmitted");

        log.Should().NotBeNull();
        log!.Status.Should().Be("Failed");
        log.RecipientEmail.Should().Be("approver1@test.com");
        log.FailureReason.Should().Be("SMTP failed.");

        email.Sent.Should().BeEmpty();
    }
}
