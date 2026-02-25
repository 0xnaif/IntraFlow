using FluentAssertions;
using IntraFlow.Domain.Requests;

namespace IntraFlow.Tests.Domain;

public class RequestWorkflowTests
{
    private static Request CreateDraft(
        string creatorId = "user-creator",
        int requestTypeId = 1)
    {
        return new Request(
            title: "Test Title",
            description: "Test Description",
            priority: RequestPriority.Medium,
            requestTypeId: requestTypeId,
            createdByUserId: creatorId);
    }

    [Fact]
    public void Draft_can_be_submitted()
    {
        var request = CreateDraft();

        request.Submit();

        request.Status.Should().Be(RequestStatus.Submitted);
        request.SubmittedAt.Should().NotBeNull();
    }

    [Fact]
    public void Non_draft_cannot_be_submitted()
    {
        var request = CreateDraft();
        request.Submit();

        var act = () => request.Submit();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*draft*");
    }

    [Fact]
    public void Submitted_can_start_review_and_assign_approver()
    {
        var request = CreateDraft();
        request.Submit();

        request.StartReview("approver-1");

        request.Status.Should().Be(RequestStatus.InReview);
        request.AssignedApproverUserId.Should().Be("approver-1");
        request.InReviewAt.Should().NotBeNull();
    }

    [Fact]
    public void Start_review_is_only_allowed_from_submitted()
    {
        var request = CreateDraft();

        var act = () => request.StartReview("approver-1");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*submitted*");
    }

    [Fact]
    public void In_review_can_be_approved_only_by_assigned_approver()
    {
        var request = CreateDraft();
        request.Submit();
        request.StartReview("approver-1");

        var decidedAt = DateTime.UtcNow;

        request.Approve("approver-1", decidedAt);

        request.Status.Should().Be(RequestStatus.Approved);
        request.DecisionAt.Should().Be(decidedAt);
    }

    [Fact]
    public void Approve_fails_if_not_assigned_approver()
    {
        var request = CreateDraft();
        request.Submit();
        request.StartReview("approver-1");

        var act = () => request.Approve("approver-2", DateTime.UtcNow);

        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("*assigned*");
    }

    [Fact]
    public void In_review_can_be_rejected_only_by_assigned_approver()
    {
        var request = CreateDraft();
        request.Submit();
        request.StartReview("approver-1");

        var decidedAt = DateTime.UtcNow;

        request.Reject("approver-1", decidedAt);

        request.Status.Should().Be(RequestStatus.Rejected);
        request.DecisionAt.Should().Be(decidedAt);
    }

    [Fact]
    public void Cannot_approve_if_not_in_review()
    {
        var request = CreateDraft();
        request.Submit();

        var act = () => request.Approve("approver-1", DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*review*");
    }

    [Fact]
    public void Cannot_reject_if_not_in_review()
    {
        var request = CreateDraft();
        request.Submit();

        var act = () => request.Reject("approver-1", DateTime.UtcNow);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*review*");
    }
}
