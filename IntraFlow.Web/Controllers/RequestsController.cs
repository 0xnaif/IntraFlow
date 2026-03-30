using IntraFlow.Application.Abstractions;
using IntraFlow.Application.Requests.Commands.AddAttachment;
using IntraFlow.Application.Requests.Commands.AddComment;
using IntraFlow.Application.Requests.Commands.ApproveRequest;
using IntraFlow.Application.Requests.Commands.CancelRequest;
using IntraFlow.Application.Requests.Commands.CreateRequest;
using IntraFlow.Application.Requests.Commands.RejectRequest;
using IntraFlow.Application.Requests.Commands.StartReview;
using IntraFlow.Application.Requests.Commands.SubmitRequest;
using IntraFlow.Application.Requests.Queries.ApproverRequests;
using IntraFlow.Application.Requests.Queries.GetComments;
using IntraFlow.Application.Requests.Queries.GetRequestDetails;
using IntraFlow.Application.Requests.Queries.MyRequests;
using IntraFlow.Web.Models.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IntraFlow.Web.Controllers;

[Authorize]
public sealed class RequestsController : Controller
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailSender _emailSender;
    private readonly IUserLookupService _userLookupService;

    public RequestsController(
        IAppDbContext db,
        ICurrentUserService currentUser,
        IEmailSender emailSender,
        IUserLookupService userLookupService)
    {
        _db = db;
        _currentUser = currentUser;
        _emailSender = emailSender;
        _userLookupService = userLookupService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var handler = new GetMyRequestsHandler(_db, _currentUser);

        var requests = await handler.Handle();

        return View(requests);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new CreateRequestViewModel
        {
            RequestTypes = await _db.RequestTypes
                .Select(x => new RequestTypeOption
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRequestViewModel vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            vm.RequestTypes = await _db.RequestTypes
                .Select(x => new RequestTypeOption
                {
                    Id = x.Id,
                    Name = x.Name
                })
                .ToListAsync();

            return View(vm);
        }

        var handler = new CreateRequestHandler(_db, _currentUser);

        var requestId = await handler.Handle(new CreateRequestCommand(
            Title: vm.Title,
            Description: vm.Description,
            Priority: vm.Priority,
            RequestTypeId: vm.RequestTypeId
        ));

        await SaveAttachmentsAsync(requestId, vm.Attachments, ct);

        if (vm.SubmitAction == "Submit")
        {
            var submitHandler = new SubmitRequestHandler(_db, _currentUser, _emailSender);
            await submitHandler.Handle(new SubmitRequestCommand(requestId));
        }

        return RedirectToAction(nameof(Details), new { requestId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAttachments(int requestId, List<IFormFile> attachments, CancellationToken ct)
    {
        await SaveAttachmentsAsync(requestId, attachments, ct);

        return RedirectToAction(nameof(Details), new { requestId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Submit(int requestId)
    {
        var handler = new SubmitRequestHandler(_db, _currentUser, _emailSender);
        await handler.Handle(new SubmitRequestCommand(requestId));

        return RedirectToAction(nameof(Details), new { requestId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Cancel(int requestId)
    {
        var handler = new CancelRequestHandler(_db, _currentUser);
        await handler.Handle(new CancelRequestCommand(requestId));

        return RedirectToAction(nameof(Details), new { requestId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanApprove")]
    public async Task<IActionResult> StartReview(int requestId)
    {
        var handler = new StartReviewHandler(_db, _currentUser);
        await handler.Handle(new StartReviewCommand(requestId));

        return RedirectToAction(nameof(Details), new { requestId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanApprove")]
    public async Task<IActionResult> Approve(int requestId)
    {
        var handler = new ApproveRequestHandler(_db, _currentUser, _emailSender);

        await handler.Handle(new ApproveRequestCommand(requestId));

        return RedirectToAction(nameof(Details), new { requestId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanApprove")]
    public async Task<IActionResult> Reject(int requestId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            ModelState.AddModelError("reason", "Rejection reason is required.");
            return RedirectToAction(nameof(Details), new { requestId });
        }

        reason = reason.Trim();

        var handler = new RejectRequestHandler(_db, _currentUser, _emailSender);
        await handler.Handle(new RejectRequestCommand(requestId, reason));

        return RedirectToAction(nameof(Details), new { requestId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int requestId, string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            return RedirectToAction(nameof(Details), new { requestId });
        }

        comment = comment.Trim();

        var handler = new AddRequestCommentHandler(_db, _currentUser);

        await handler.Handle(new AddRequestCommentCommand(requestId, comment));

        return RedirectToAction(nameof(Details), new { requestId });
    }

    [HttpGet]
    public async Task<IActionResult> Details(int requestId)
    {
        var requesthandler = new GetRequestDetailsHandler(_db, _currentUser);

        try
        {
            var request = await requesthandler.Handle(new GetRequestDetailsQuery(requestId));

            if (request is null)
                return NotFound();

            var commentsHandler = new GetRequestCommentsHandler(_db, _userLookupService);
            var comments = await commentsHandler.Handle(new GetRequestCommentsQuery(requestId));

            var vm = new RequestDetailsPageViewModel
            {
                Request = request,
                Comments = comments
            };

            return View(vm);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [Authorize(Policy = "CanApprove")]
    [HttpGet]
    public async Task<IActionResult> Pending()
    {
        var handler = new GetRequestsForApproverHandler(_db, _currentUser);

        var requests = await handler.Handle();

        return View(requests);
    }

    private async Task SaveAttachmentsAsync(int requestId, List<IFormFile>? attachments, CancellationToken ct = default)
    {
        if (attachments is null || attachments.Count == 0)
            return;

        var handler = new AddRequestAttachmentHandler(_db, _currentUser);

        foreach (var attachment in attachments.Where(x => x is not null && x.Length > 0))
        {
            await using var stream = new MemoryStream();
            await attachment.CopyToAsync(stream, ct);

            await handler.Handle(new AddRequestAttachmentCommand(
                RequestId: requestId,
                FileName: attachment.FileName,
                ContentType: attachment.ContentType ?? "application/octet-stream",
                FileSizeBytes: (int)attachment.Length,
                FileData: stream.ToArray()
            ), ct);
        }
    }
}