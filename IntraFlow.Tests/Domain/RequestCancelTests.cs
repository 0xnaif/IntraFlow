using FluentAssertions;
using IntraFlow.Domain.Requests;

namespace IntraFlow.Tests.Domain;

public class RequestCancelTests
{
    private static Request CreateDraft(string creatorId = "user-creator")
    {
        return new Request(
            title: "Cancel Test",
            description: "Cancel Desc",
            priority: RequestPriority.Low,
            requestTypeId: 1,
            createdByUserId: creatorId);
    }

    [Fact]
    public void Creator_can_cancel_draft()
    {
        var request = CreateDraft("user-1");

        request.Cancel("user-1");

        request.Status.Should().Be(RequestStatus.Cancelled);
        request.CancelledAt.Should().NotBeNull();
    }

    [Fact]
    public void Non_creator_cannot_cancel()
    {
        var request = CreateDraft("user-1");

        var act = () => request.Cancel("user-2");

        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("*creator*");
    }

    [Fact]
    public void Cannot_cancel_after_approved()
    {
        var request = CreateDraft("user-1");
        request.Submit();
        request.StartReview("approver-1");
        request.Approve("approver-1", DateTime.UtcNow);

        var act = () => request.Cancel("user-1");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot be cancelled*");
    }

    [Fact]
    public void Cannot_cancel_after_rejected()
    {
        var request = CreateDraft("user-1");
        request.Submit();
        request.StartReview("approver-1");
        request.Reject("approver-1", DateTime.UtcNow);

        var act = () => request.Cancel("user-1");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot be cancelled*");
    }
}
