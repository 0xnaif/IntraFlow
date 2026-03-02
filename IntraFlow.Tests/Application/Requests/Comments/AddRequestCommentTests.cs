using FluentAssertions;
using IntraFlow.Application.Requests.Commands.AddComment;
using IntraFlow.Application.Requests.Commands.ApproveRequest;
using IntraFlow.Application.Requests.Commands.CreateRequest;
using IntraFlow.Application.Requests.Commands.StartReview;
using IntraFlow.Application.Requests.Commands.SubmitRequest;
using IntraFlow.Domain.Requests;
using IntraFlow.Tests.Application.Fakes;
using IntraFlow.Tests.Application.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntraFlow.Tests.Application.Requests.Comments;

public class AddRequestCommentTests
{
    [Fact]
    public async Task Creator_can_add_comment()
    {
        await using var db = DbFactory.Create();

        var creator = new FakeCurrentUserService { UserId = "creator" };
        var approver = "approver";

        db.RequestTypes.Add(new RequestType("Leave", "Test", approver));
        await db.SaveChangesAsync();

        var requestTypeId = await db.RequestTypes.Select(x => x.Id).FirstAsync();

        var createHandler = new CreateRequestHandler(db, creator);
        var requestId = await createHandler.Handle(new(
            Title: "Test",
            Description: "Desc",
            Priority: RequestPriority.Medium,
            RequestTypeId: requestTypeId
        ));

        var submitHandler = new SubmitRequestHandler(db, creator, new FakeEmailSender());
        await submitHandler.Handle(new(requestId));

        var commentHandler = new AddRequestCommentHandler(db, creator);
        await commentHandler.Handle(new(requestId, "Hello"));

        db.RequestComments.Count().Should().Be(1);
        db.AuditLogs.Any(x => x.ActionType == "CommentAdded").Should().BeTrue();
    }

    [Fact]
    public async Task Cannot_comment_after_approval()
    {
        await using var db = DbFactory.Create();

        var creator = new FakeCurrentUserService { UserId = "creator" };
        var email = new FakeEmailSender();
        var approverUser = new FakeCurrentUserService { UserId = "approver", Roles = { "Approver" } };

        db.RequestTypes.Add(new RequestType("Leave", "Test", "approver"));
        await db.SaveChangesAsync();

        var requestTypeId = await db.RequestTypes.Select(x => x.Id).FirstAsync();

        var createHandler = new CreateRequestHandler(db, creator);
        var requestId = await createHandler.Handle(new(
            Title: "Test",
            Description: "Desc",
            Priority: RequestPriority.Medium,
            RequestTypeId: requestTypeId
        ));

        await new SubmitRequestHandler(db, creator, email)
            .Handle(new(requestId));

        await new StartReviewHandler(db, approverUser)
            .Handle(new(requestId));

        await new ApproveRequestHandler(db, approverUser, email)
            .Handle(new(requestId));

        var commentHandler = new AddRequestCommentHandler(db, creator);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            commentHandler.Handle(new(requestId, "Should fail")));
    }

    [Fact]
    public async Task Unauthorized_user_cannot_comment()
    {
        await using var db = DbFactory.Create();

        var creator = new FakeCurrentUserService { UserId = "creator" };
        var attacker = new FakeCurrentUserService { UserId = "attacker" };

        db.RequestTypes.Add(new RequestType("Leave", "Test", "approver"));
        await db.SaveChangesAsync();

        var requestTypeId = await db.RequestTypes.Select(x => x.Id).FirstAsync();

        var createHandler = new CreateRequestHandler(db, creator);
        var requestId = await createHandler.Handle(new(
            Title: "Test",
            Description: "Desc",
            Priority: RequestPriority.Medium,
            RequestTypeId: requestTypeId
        ));

        await new SubmitRequestHandler(db, creator, new FakeEmailSender())
            .Handle(new(requestId));

        var commentHandler = new AddRequestCommentHandler(db, attacker);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            commentHandler.Handle(new(requestId, "Hack")));
    }
}
