using Hotel.Data.Models;
using Hotel.Web.ViewModels.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByNameAsync(model.UserName).ConfigureAwait(false);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        // Inactive or dismissed staff must not access the system.
        if (!user.IsActive || user.DismissalDate.HasValue)
        {
            ModelState.AddModelError(
                string.Empty,
                "This account is inactive or dismissed. Contact an administrator.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
                model.UserName,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true)
            .ConfigureAwait(false);

        if (result.Succeeded)
        {
            return RedirectToLocal(returnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account locked. Try again later.");
        }
        else
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync().ConfigureAwait(false);
        return RedirectToAction(nameof(HomeController.Index), "Home");
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction(nameof(HomeController.Index), "Home");
    }
}
