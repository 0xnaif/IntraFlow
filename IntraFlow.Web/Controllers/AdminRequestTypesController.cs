using IntraFlow.Application.Abstractions;
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
    private readonly IAppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminRequestTypesController(IAppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var handler = new GetRequestTypesHandler(_db);
        var items = await handler.Handle(new GetRequestTypesQuery(), ct);

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

        var handler = new CreateRequestTypeHandler(_db);
        await handler.Handle(new CreateRequestTypeCommand(
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
