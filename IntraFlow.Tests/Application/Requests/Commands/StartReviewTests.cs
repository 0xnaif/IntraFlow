using FluentAssertions;
using IntraFlow.Application.Requests.Commands.CreateRequest;
using IntraFlow.Application.Requests.Commands.StartReview;
using IntraFlow.Application.Requests.Commands.SubmitRequest;
using IntraFlow.Domain.Requests;
using IntraFlow.Tests.Application.Fakes;
using IntraFlow.Tests.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace IntraFlow.Tests.Application.Requests.Commands;

public class StartReviewTests
{
    private readonly ILogger<SubmitRequestHandler> _logger;

    public StartReviewTests()
    {
        _logger = NullLogger<SubmitRequestHandler>.Instance;
    }
    [Fact]
    public async Task StartReview_sends_email_to_request_creator_and_creates_sent_notification_log()
    {
        await using var db = DbFactory.Create();

        var creator = new FakeCurrentUserService { UserId = "user-creator" };
        var approver = new FakeCurrentUserService { UserId = "approver-1" };
        var email = new FakeEmailSender();
        var userLookup = new FakeUserLookupService();

        userLookup.SetUser("user-creator", "Creator User", "creator@test.com");
        userLookup.SetUser("approver-1", "Approver One", "approver1@test.com");

        db.RequestTypes.Add(new RequestType("Leave Request", "Test", defaultApproverUserId: "approver-1"));
        await db.SaveChangesAsync();

        var requestTypeId = db.RequestTypes.Select(x => x.Id).First();

        var createHandler = new CreateRequestHandler(db, creator);
        var requestId = await createHandler.Handle(new CreateRequestCommand(
            Title: "Need leave",
            Description: "Personal",
            Priority: RequestPriority.Medium,
            RequestTypeId: requestTypeId
        ));

        var submitHandler = new SubmitRequestHandler(db, creator, email, userLookup, _logger);
        await submitHandler.Handle(new SubmitRequestCommand(requestId));

        email.Sent.Clear();

        var startReviewHandler = new StartReviewHandler(db, approver, email, userLookup);
        await startReviewHandler.Handle(new StartReviewCommand(requestId));

        var request = await db.Requests.FirstAsync(x => x.Id == requestId);
        request.Status.Should().Be(RequestStatus.InReview);

        var log = await db.NotificationLogs
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(x => x.RequestId == requestId && x.EventType == "RequestReviewStarted");

        log.Should().NotBeNull();
        log!.Status.Should().Be("Sent");
        log.RecipientEmail.Should().Be("creator@test.com");

        email.Sent.Should().HaveCount(1);
        email.Sent[0].To.Should().Be("creator@test.com");
    }
}