using Hotel.Data.Models;
using Hotel.Services.Interfaces;
using Hotel.Web.Authorization;
using Hotel.Web.ViewModels.Shared;
using Hotel.Web.ViewModels.Staff;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Web.Controllers;

[Authorize(Roles = HotelRoles.Admin)]
public class StaffUsersController : Controller
{
    private readonly IStaffUserService _staffUserService;
    private readonly UserManager<ApplicationUser> _userManager;

    public StaffUsersController(
        IStaffUserService staffUserService,
        UserManager<ApplicationUser> userManager)
    {
        _staffUserService = staffUserService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        int page = 1,
        int pageSize = 10,
        string? userName = null,
        string? name = null,
        string? email = null)
    {
        pageSize = pageSize is 10 or 25 or 50 ? pageSize : 10;
        var result = await _staffUserService.GetPagedAsync(page, pageSize, userName, name, email).ConfigureAwait(false);
        var users = new List<StaffUserListItemViewModel>();
        foreach (var u in result.Items)
        {
            var roles = await _userManager.GetRolesAsync(u).ConfigureAwait(false);
            users.Add(MapListItem(u, roles));
        }

        var vm = new StaffUserIndexViewModel
        {
            UserNameFilter = userName,
            NameFilter = name,
            EmailFilter = email,
            PageSize = pageSize,
            Users = users,
            Pager = new PagerViewModel
            {
                Page = result.Page,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount
            }
        };
        return View(vm);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new StaffUserCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(StaffUserCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            Email = model.Email,
            EmailConfirmed = true,
            FirstName = model.FirstName,
            MiddleName = model.MiddleName,
            LastName = model.LastName,
            EGN = model.EGN,
            PhoneNumber = model.PhoneNumber,
            HireDate = model.HireDate,
            IsActive = model.IsActive,
            DismissalDate = model.DismissalDate
        };

        var (ok, errors) = await _staffUserService.CreateAsync(user, model.Password, model.SelectedRoles)
            .ConfigureAwait(false);
        if (!ok)
        {
            foreach (var e in errors)
            {
                ModelState.AddModelError(string.Empty, e);
            }

            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await _staffUserService.GetByIdAsync(id).ConfigureAwait(false);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _staffUserService.GetRolesAsync(user).ConfigureAwait(false);
        var vm = new StaffUserEditViewModel
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            MiddleName = user.MiddleName,
            LastName = user.LastName,
            EGN = user.EGN,
            PhoneNumber = user.PhoneNumber ?? string.Empty,
            HireDate = user.HireDate,
            IsActive = user.IsActive,
            DismissalDate = user.DismissalDate,
            SelectedRoles = roles.ToList()
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(StaffUserEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _staffUserService.GetByIdAsync(model.Id).ConfigureAwait(false);
        if (user == null)
        {
            return NotFound();
        }

        user.UserName = model.UserName;
        user.Email = model.Email;
        user.FirstName = model.FirstName;
        user.MiddleName = model.MiddleName;
        user.LastName = model.LastName;
        user.EGN = model.EGN;
        user.PhoneNumber = model.PhoneNumber;
        user.HireDate = model.HireDate;
        user.IsActive = model.IsActive;
        user.DismissalDate = model.DismissalDate;

        var (updateOk, updateErrors) = await _staffUserService.UpdateAsync(user).ConfigureAwait(false);
        if (!updateOk)
        {
            foreach (var e in updateErrors)
            {
                ModelState.AddModelError(string.Empty, e);
            }

            return View(model);
        }

        var (rolesOk, roleErrors) = await _staffUserService.SetRolesAsync(user, model.SelectedRoles)
            .ConfigureAwait(false);
        if (!rolesOk)
        {
            foreach (var e in roleErrors)
            {
                ModelState.AddModelError(string.Empty, e);
            }

            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await _staffUserService.GetByIdAsync(id).ConfigureAwait(false);
        if (user == null)
        {
            return NotFound();
        }

        var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
        return View(MapListItem(user, roles));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var (ok, errors) = await _staffUserService.DeleteAsync(id).ConfigureAwait(false);
        if (!ok)
        {
            foreach (var e in errors)
            {
                ModelState.AddModelError(string.Empty, e);
            }

            var user = await _staffUserService.GetByIdAsync(id).ConfigureAwait(false);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
                return View("Delete", MapListItem(user, roles));
            }

            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    private static StaffUserListItemViewModel MapListItem(ApplicationUser u, IList<string> roles)
    {
        var middle = string.IsNullOrWhiteSpace(u.MiddleName) ? string.Empty : u.MiddleName + " ";
        return new StaffUserListItemViewModel
        {
            Id = u.Id,
            UserName = u.UserName ?? string.Empty,
            FullName = $"{u.FirstName} {middle}{u.LastName}".Trim(),
            Email = u.Email ?? string.Empty,
            IsActive = u.IsActive,
            DismissalDate = u.DismissalDate,
            Roles = roles
        };
    }
}
