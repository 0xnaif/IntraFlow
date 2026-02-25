using IntraFlow.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Application.Requests.Commands.StartReview;

public sealed class StartReviewHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public StartReviewHandler(IAppDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(StartReviewCommand cmd, CancellationToken ct = default)
    {
        if (!_currentUser.IsInRole("Approver") && !_currentUser.IsInRole("Admin"))
            throw new UnauthorizedAccessException("Only approvers/admins can start review.");

        var request = await _db.Requests.FirstOrDefaultAsync(x => x.Id == cmd.RequestId, ct)
            ?? throw new InvalidOperationException("Request not found.");

        if (request.AssignedApproverUserId is null)
        {
            var type = await _db.RequestTypes.FirstAsync(x => x.Id == request.RequestTypeId, ct);
            if (!string.IsNullOrWhiteSpace(type.DefaultApproverUserId))
                request.StartReview(type.DefaultApproverUserId);
            else
                request.StartReview(_currentUser.UserId); // fallback
        }
        else
        {
            request.StartReview(request.AssignedApproverUserId);
        }

        await _db.SaveChangesAsync(ct);
    }
}
