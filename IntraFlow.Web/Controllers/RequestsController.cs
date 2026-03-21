using IntraFlow.Application.Abstractions;
using IntraFlow.Application.Requests.Commands.ApproveRequest;
using IntraFlow.Application.Requests.Commands.CancelRequest;
using IntraFlow.Application.Requests.Commands.CreateRequest;
using IntraFlow.Application.Requests.Commands.RejectRequest;
using IntraFlow.Application.Requests.Commands.StartReview;
using IntraFlow.Application.Requests.Commands.SubmitRequest;
using IntraFlow.Application.Requests.Queries.ApproverRequests;
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

    public RequestsController(
        IAppDbContext db,
        ICurrentUserService currentUser,
        IEmailSender emailSender)
    {
        _db = db;
        _currentUser = currentUser;
        _emailSender = emailSender;
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
    public async Task<IActionResult> Create(CreateRequestViewModel vm)
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

        if (vm.SubmitAction == "Submit")
        {
            var submitHandler = new SubmitRequestHandler(_db, _currentUser, _emailSender);
            await submitHandler.Handle(new SubmitRequestCommand(requestId));
        }

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

    [HttpGet]
    public async Task<IActionResult> Details(int requestId)
    {
        var handler = new GetRequestDetailsHandler(_db);

        var request = await handler.Handle(new GetRequestDetailsQuery(requestId));

        if (request is null)
            return NotFound();

        return View(request);
    }

    [Authorize(Policy = "CanApprove")]
    [HttpGet]
    public async Task<IActionResult> Pending()
    {
        var handler = new GetRequestsForApproverHandler(_db, _currentUser);

        var requests = await handler.Handle();

        return View(requests);
    }
}