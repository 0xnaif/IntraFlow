using IntraFlow.Application.Abstractions;
using IntraFlow.Application.Audit.DTOs;
using IntraFlow.Application.Audit.Queries.GetAuditEntriesForEntity;
using IntraFlow.Application.Requests.Commands.AddAttachment;
using IntraFlow.Application.Requests.Commands.AddComment;
using IntraFlow.Application.Requests.Commands.ApproveRequest;
using IntraFlow.Application.Requests.Commands.CancelRequest;
using IntraFlow.Application.Requests.Commands.CreateRequest;
using IntraFlow.Application.Requests.Commands.DeleteAttachment;
using IntraFlow.Application.Requests.Commands.RejectRequest;
using IntraFlow.Application.Requests.Commands.StartReview;
using IntraFlow.Application.Requests.Commands.SubmitRequest;
using IntraFlow.Application.Requests.DTOs;
using IntraFlow.Application.Requests.Queries.ApproverRequests;
using IntraFlow.Application.Requests.Queries.GetAttachments;
using IntraFlow.Application.Requests.Queries.GetComments;
using IntraFlow.Application.Requests.Queries.GetRequestDetails;
using IntraFlow.Application.Requests.Queries.MyRequests;
using IntraFlow.Domain.Requests;
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

    private const string SuccessMessageKey = "RequestDetails.Success";
    private const string ErrorMessageKey = "RequestDetails.Error";


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

        try
        {
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
                var submitHandler = new SubmitRequestHandler(_db, _currentUser, _emailSender, _userLookupService);
                await submitHandler.Handle(new SubmitRequestCommand(requestId));

                SetSuccessMessage("Request created and submitted successfully.");
            }
            else
            {
                SetSuccessMessage("Request created successfully.");
            }

            return RedirectToAction(nameof(Details), new { requestId });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("AccessDenied", "Home");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAttachments(int requestId, List<IFormFile> attachments, CancellationToken ct)
    {
        if (attachments is null || attachments.Count == 0)
        {
            SetErrorMessage("Please select at least one file to upload.");
            return RedirectToAction(nameof(Details), new { requestId });
        }
        try
        {
            await SaveAttachmentsAsync(requestId, attachments, ct);

            SetSuccessMessage("Attachment(s) uploaded successfully.");
            return RedirectToAction(nameof(Details), new { requestId });
        }
        catch (ArgumentException ex)
        {
            SetErrorMessage(ex.Message);
            return RedirectToAction(nameof(Details), new { requestId });
        }
        catch (InvalidOperationException ex)
        {
            SetErrorMessage(ex.Message);
            return RedirectToAction(nameof(Details), new { requestId });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("AccessDenied", "Home");;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAttachment(int attachmentId, int requestId, CancellationToken ct = default)
    {
        var handler = new DeleteAttachmentHandler(_db, _currentUser);

        try
        {
            await handler.Handle(new DeleteAttachmentCommand(attachmentId), ct);

            SetSuccessMessage("Attachment deleted successfully.");
            return RedirectToAction(nameof(Details), new { requestId });
        }
        catch (InvalidOperationException ex)
        {
            SetErrorMessage(ex.Message);
            return RedirectToAction(nameof(Details), new { requestId });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("AccessDenied", "Home");;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Submit(int requestId)
    {
        var handler = new SubmitRequestHandler(_db, _currentUser, _emailSender, _userLookupService);

        try
        {
            await handler.Handle(new SubmitRequestCommand(requestId));

            SetSuccessMessage("Request submitted successfully.");
            return RedirectToAction(nameof(Details), new { requestId });
        }
        catch (InvalidOperationException ex)
        {
            SetErrorMessage(ex.Message);
            return RedirectToAction(nameof(Details), new { requestId });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("AccessDenied", "Home");;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Cancel(int requestId)
    {
        var handler = new CancelRequestHandler(_db, _currentUser);

        try
        {
            await handler.Handle(new CancelRequestCommand(requestId));

            SetSuccessMessage("Request cancelled successfully.");
            return RedirectToAction(nameof(Details), new { requestId });
        }
        catch (InvalidOperationException ex)
        {
            SetErrorMessage(ex.Message);
            return RedirectToAction(nameof(Details), new { requestId });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("AccessDenied", "Home");;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanApprove")]
    public async Task<IActionResult> StartReview(int requestId)
    {
        var handler = new StartReviewHandler(_db, _currentUser, _emailSender, _userLookupService);

        try
        {
            await handler.Handle(new StartReviewCommand(requestId));

            SetSuccessMessage("Review started successfully.");
            return RedirectToAction(nameof(Details), new { requestId });
        }
        catch (InvalidOperationException ex)
        {
            SetErrorMessage(ex.Message);
            return RedirectToAction(nameof(Details), new { requestId });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("AccessDenied", "Home");;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanApprove")]
    public async Task<IActionResult> Approve(int requestId)
    {
        var handler = new ApproveRequestHandler(_db, _currentUser, _emailSender, _userLookupService);

        try
        {
            await handler.Handle(new ApproveRequestCommand(requestId));

            SetSuccessMessage("Request approved successfully.");
            return RedirectToAction(nameof(Details), new { requestId });
        }
        catch (InvalidOperationException ex)
        {
            SetErrorMessage(ex.Message);
            return RedirectToAction(nameof(Details), new { requestId });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("AccessDenied", "Home");;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "CanApprove")]
    public async Task<IActionResult> Reject(int requestId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            SetErrorMessage("Rejection reason is required.");
            return RedirectToAction(nameof(Details), new { requestId });
        }

        reason = reason.Trim();

        var handler = new RejectRequestHandler(_db, _currentUser, _emailSender, _userLookupService);

        try
        {
            await handler.Handle(new RejectRequestCommand(requestId, reason));

            SetSuccessMessage("Request rejected successfully.");
            return RedirectToAction(nameof(Details), new { requestId });
        }
        catch (InvalidOperationException ex)
        {
            SetErrorMessage(ex.Message);
            return RedirectToAction(nameof(Details), new { requestId });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("AccessDenied", "Home");;
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int requestId, string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            SetErrorMessage("Comment cannot be empty.");
            return RedirectToAction(nameof(Details), new { requestId });
        }

        comment = comment.Trim();

        var handler = new AddRequestCommentHandler(_db, _currentUser);

        try
        {
            await handler.Handle(new AddRequestCommentCommand(requestId, comment));

            SetSuccessMessage("Comment added successfully.");
            return RedirectToAction(nameof(Details), new { requestId });
        }
        catch (ArgumentException ex)
        {
            SetErrorMessage(ex.Message);
            return RedirectToAction(nameof(Details), new { requestId });
        }
        catch (InvalidOperationException ex)
        {
            SetErrorMessage(ex.Message);
            return RedirectToAction(nameof(Details), new { requestId });
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("AccessDenied", "Home");;
        }
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

            var attachmentsHandler = new GetRequestAttachmentsHandler(_db, _currentUser);
            var attachments = await attachmentsHandler.Handle(new GetRequestAttachmentsQuery(requestId));

            var auditHandler = new GetAuditEntriesForEntityHandler(_db, _userLookupService);
            var auditEntries = await auditHandler.Handle(new GetAuditEntriesForEntityQuery("Request", requestId.ToString()));

            var vm = BuildRequestDetailsPageViewModel(
                request,
                comments,
                attachments,
                auditEntries);

            return View(vm);
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("AccessDenied", "Home");;
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

    [HttpGet]
    public async Task<IActionResult> ViewAttachment(int attachmentId)
    {
        var handler = new GetAttachmentFileHandler(_db, _currentUser);

        try
        {
            var result = await handler.Handle(new GetAttachmentFileQuery(attachmentId));

            return File(
                result.Data,
                result.ContentType,
                enableRangeProcessing: true);
        }
        catch (InvalidOperationException)
        {
            return NotFound();
        }
        catch (UnauthorizedAccessException)
        {
            return RedirectToAction("AccessDenied", "Home");;
        }
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

    private void SetSuccessMessage(string message)
    {
        TempData[SuccessMessageKey] = message;
    }

    private void SetErrorMessage(string message)
    {
        TempData[ErrorMessageKey] = message;
    }

    private RequestDetailsPageViewModel BuildRequestDetailsPageViewModel(
        RequestDetailsDto request,
        List<RequestCommentDto> comments,
        List<RequestAttachmentDto> attachments,
        List<AuditEntryDto> auditEntries)
    {
        var currentUserId = _currentUser.UserId;

        var isCreator = request.CreatedByUserId == currentUserId;
        var isAssignedApprover = request.AssignedApproverUserId == currentUserId;
        var isAuthorizedViewer = isCreator || isAssignedApprover;

        var isDraft = request.Status == RequestStatus.Draft;
        var isSubmitted = request.Status == RequestStatus.Submitted;
        var isInReview = request.Status == RequestStatus.InReview;
        var isFinalized =
            request.Status == RequestStatus.Approved ||
            request.Status == RequestStatus.Rejected ||
            request.Status == RequestStatus.Cancelled;

        var canSubmit = isCreator && isDraft;
        var canCancel = isCreator && isDraft;
        var canStartReview = isAssignedApprover && isSubmitted;
        var canApprove = isAssignedApprover && isInReview;
        var canReject = isAssignedApprover && isInReview;

        var canComment = isAuthorizedViewer && (isSubmitted || isInReview);
        var canUploadAttachment = isCreator && (isDraft || isSubmitted || isInReview);
        var canDeleteAttachment = isCreator && isDraft;

        return new RequestDetailsPageViewModel
        {
            Request = request,
            Comments = comments,
            Attachments = attachments,
            AuditEntries = auditEntries,

            IsAuthorizedViewer = isAuthorizedViewer,
            IsCreator = isCreator,
            IsAssignedApprover = isAssignedApprover,

            CanSubmit = canSubmit,
            CanCancel = canCancel,
            CanStartReview = canStartReview,
            CanApprove = canApprove,
            CanReject = canReject,

            CanComment = canComment,
            CanUploadAttachment = canUploadAttachment,
            CanDeleteAttachment = canDeleteAttachment,

            HasAnyAvailableAction =
                canSubmit ||
                canCancel ||
                canStartReview ||
                canApprove ||
                canReject,

            SuccessMessage = TempData[SuccessMessageKey]?.ToString(),
            ErrorMessage = TempData[ErrorMessageKey]?.ToString()
        };

    }
}