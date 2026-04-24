using FluentAssertions;
using IntraFlow.Application.Requests.Commands.ApproveRequest;
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

public class ApproveRequestTests
{
    private readonly ILogger<SubmitRequestHandler> _logger;

    public ApproveRequestTests()
    {
        _logger = NullLogger<SubmitRequestHandler>.Instance;
    }
    [Fact]
    public async Task Approve_sends_email_to_creator_and_logs_notification()
    {
        await using var db = DbFactory.Create();

        var creator = new FakeCurrentUserService { UserId = "creator" };
        var approver = new FakeCurrentUserService { UserId = "approver" };
        var email = new FakeEmailSender();
        var userLookup = new FakeUserLookupService();

        userLookup.SetUser("creator", "Creator", "creator@test.com");
        userLookup.SetUser("approver", "Approver", "approver@test.com");

        db.RequestTypes.Add(new RequestType("Test", "Test", "approver"));
        await db.SaveChangesAsync();

        var requestTypeId = db.RequestTypes.Select(x => x.Id).First();

        var create = new CreateRequestHandler(db, creator);
        var requestId = await create.Handle(new CreateRequestCommand(
            "Title", "Desc", RequestPriority.Medium, requestTypeId));

        var submit = new SubmitRequestHandler(db, creator, email, userLookup, _logger);
        await submit.Handle(new SubmitRequestCommand(requestId));

        var start = new StartReviewHandler(db, approver, email, userLookup);
        await start.Handle(new StartReviewCommand(requestId));

        email.Sent.Clear();

        var approve = new ApproveRequestHandler(db, approver, email, userLookup);
        await approve.Handle(new ApproveRequestCommand(requestId));

        var log = await db.NotificationLogs
            .FirstOrDefaultAsync(x => x.EventType == "RequestApproved");


        log.Should().NotBeNull();
        log!.RecipientEmail.Should().Be("creator@test.com");

        email.Sent.Should().HaveCount(1);
    }
}
