using IntraFlow.Application.Common.Constants;
using IntraFlow.Infrastructure.Identity;
using IntraFlow.Web.Models.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IntraFlow.Web.Controllers;

public class AdminUsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminUsersController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var currentAdminEmail = User.Identity!.Name;

        var users = _userManager.Users
            .Where(x => x.Email != currentAdminEmail)
            .ToList();

        var result = new List<UserListItemViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            result.Add(new UserListItemViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = roles.FirstOrDefault() ?? ""
            });
        }

        return View(result);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var vm = new CreateUserViewModel
        {
            Roles = _roleManager.Roles // How to exclude Admin role?
                .Where(x => x.Name != Roles.Admin)
                .Select(x => x.Name!)
                .ToList()
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Roles = _roleManager.Roles.Select(x => x.Name!).ToList(); // Why is it needed to define vm.Roles again?
            return View(vm);
        }

        var user = new ApplicationUser
        {
            UserName = vm.Email,
            Email = vm.Email,
            FullName = vm.Name
        };

        var result = await _userManager.CreateAsync(user, "Temp123!");

        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Failed to create user");
            vm.Roles = _roleManager.Roles.Select(x => x.Name!).ToList();
            return View(vm);
        }

        await _userManager.AddToRoleAsync(user, vm.Role);

        return RedirectToAction(nameof(Index));
    }
}
