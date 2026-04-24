using IntraFlow.Application.Common.Constants;
using IntraFlow.Application.RequestTypes.Commands.CreateRequestType;
using IntraFlow.Application.RequestTypes.Queries;
using IntraFlow.Infrastructure.Identity;
using IntraFlow.Web.Models.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IntraFlow.Web.Controllers;

[Authorize(Policy = "IsAdmin")]
public class AdminRequestTypesController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly GetRequestTypesHandler _getRequestTypesHandler;
    private readonly CreateRequestTypeHandler _createRequestTypeHandler;

    public AdminRequestTypesController(
        UserManager<ApplicationUser> userManager,
        GetRequestTypesHandler getRequestTypesHandler,
        CreateRequestTypeHandler createRequestTypeHandler)
    {
        _userManager = userManager;
        _getRequestTypesHandler = getRequestTypesHandler;
        _createRequestTypeHandler = createRequestTypeHandler;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var items = await _getRequestTypesHandler.Handle(new GetRequestTypesQuery(), ct);

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new CreateRequestTypeViewModel
        {
            Approvers = await GetApproverOptionsAsync()
        };

        return View(vm);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRequestTypeViewModel vm,  CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            vm.Approvers = await GetApproverOptionsAsync();
            return View(vm);
        }

        await _createRequestTypeHandler.Handle(new CreateRequestTypeCommand(
            Name: vm.Name,
            Description: vm.Description,
            DefaultApproverUserId: vm.DefaultApproverUserId), ct);

        return RedirectToAction(nameof(Index));
    }

    private async Task<List<ApproverOption>> GetApproverOptionsAsync()
    {
        var approvers = await _userManager.GetUsersInRoleAsync(Roles.Approver);

        return approvers
            .OrderBy(x => x.FullName)
            .Select(x =>  new ApproverOption
            {
                Id = x.Id,
                FullName = x.FullName,
                Email = x.Email ?? string.Empty,
            })
            .ToList();
    }
}
