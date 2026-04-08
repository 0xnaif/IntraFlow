using FluentAssertions;
using IntraFlow.Application.Requests.Commands.CreateRequest;
using IntraFlow.Application.Requests.Commands.RejectRequest;
using IntraFlow.Application.Requests.Commands.StartReview;
using IntraFlow.Application.Requests.Commands.SubmitRequest;
using IntraFlow.Domain.Requests;
using IntraFlow.Tests.Application.Fakes;
using IntraFlow.Tests.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Tests.Application.Requests.Commands;

public class RejectRequestTests
{
    [Fact]
    public async Task Reject_sends_email_to_creator_and_logs_notification()
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

        var submit = new SubmitRequestHandler(db, creator, email, userLookup);
        await submit.Handle(new SubmitRequestCommand(requestId));

        var start = new StartReviewHandler(db, approver, email, userLookup);
        await start.Handle(new StartReviewCommand(requestId));

        email.Sent.Clear();

        var reject = new RejectRequestHandler(db, approver, email, userLookup);
        await reject.Handle(new RejectRequestCommand(requestId, "Not valid"));

        var log = await db.NotificationLogs
            .FirstOrDefaultAsync(x => x.EventType == "RequestRejected");

        log.Should().NotBeNull();
        log!.RecipientEmail.Should().Be("creator@test.com");

        email.Sent.Should().HaveCount(1);
    }
}
