using FluentAssertions;
using IntraFlow.Application.Audit.Queries.GetAuditEntriesForEntity;
using IntraFlow.Application.Requests.Commands.CreateRequest;
using IntraFlow.Application.Requests.Commands.StartReview;
using IntraFlow.Application.Requests.Commands.SubmitRequest;
using IntraFlow.Domain.Requests;
using IntraFlow.Tests.Application.Fakes;
using IntraFlow.Tests.Application.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Tests.Application.Audit;

public class GetAuditEntriesForEntityTests
{
    [Fact]
    public async Task Handle_returns_request_audit_entries_with_actor_names_and_readable_summaries()
    {
        await using var db = DbFactory.Create();

        var creator = new FakeCurrentUserService { UserId = "creator-1" };
        var approver = new FakeCurrentUserService { UserId = "approver-1" };
        var email = new FakeEmailSender();
        var userLookup = new FakeUserLookupService();

        userLookup.SetUser(creator.UserId, "Creator User", "creator@test.com");
        userLookup.SetUser(approver.UserId, "Approver User", "approver@test.com");

        db.RequestTypes.Add(new RequestType("Leave Request", "Test", defaultApproverUserId: "approver-1"));
        await db.SaveChangesAsync();

        var requestTypeId = db.RequestTypes.Select(x => x.Id).First();

        var createHandler = new CreateRequestHandler(db, creator);
        var requestId = await createHandler.Handle(new CreateRequestCommand(
            Title: "Need leave",
            Description: "Personal",
            Priority: RequestPriority.Medium,
            RequestTypeId: requestTypeId));

        var submitHandler = new SubmitRequestHandler(db, creator, email, userLookup);
        await submitHandler.Handle(new SubmitRequestCommand(requestId));

        var startReviewHandler = new StartReviewHandler(db, approver, email, userLookup);
        await startReviewHandler.Handle(new StartReviewCommand(requestId));

        var handler = new GetAuditEntriesForEntityHandler(db, userLookup);

        var result = await handler.Handle(new GetAuditEntriesForEntityQuery("Request", requestId.ToString()));

        result.Should().HaveCount(3);

        result[0].ActionType.Should().Be("Created");
        result[0].PerformedByDisplayName.Should().Be("Creator User");
        result[0].Summary.Should().Contain("Request created");

        result[1].ActionType.Should().Be("Submitted");
        result[1].PerformedByDisplayName.Should().Be("Creator User");
        result[1].Summary.Should().Contain("Draft");
        result[1].Summary.Should().Contain("Submitted");

        result[2].ActionType.Should().Be("ReviewStarted");
        result[2].PerformedByDisplayName.Should().Be("Approver User");
        result[2].Summary.Should().Contain("Submitted");
        result[2].Summary.Should().Contain("InReview");
    }
}
