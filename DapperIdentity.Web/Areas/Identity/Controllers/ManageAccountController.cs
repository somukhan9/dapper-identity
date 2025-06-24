using DapperIdentity.Models.Identity;
using DapperIdentity.Models.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

namespace DapperIdentity.Web.Areas.Identity.Controllers;

[Area(areaName: "Identity")]
public class ManageAccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IUserStore<ApplicationUser> userStore,
    IEmailSender emailSender,
    ILogger<ManageAccountController> logger)
    : Controller
{
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        var vm = new ProfileViewModel()
        {
            Username = user.UserName!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber
        };
        return View("Index", vm);
    }

    [HttpPost]
    public async Task<IActionResult> Index(ProfileViewModel vm, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        var shouldUpdate = false;

        if (!string.IsNullOrEmpty(vm.PhoneNumber) && !vm.PhoneNumber.Equals(user.PhoneNumber))
        {
            user.PhoneNumber = vm.PhoneNumber;
            shouldUpdate = true;
        }

        if (!string.IsNullOrEmpty(vm.FirstName) && !vm.FirstName.Equals(user.FirstName))
        {
            user.FirstName = vm.FirstName;
            shouldUpdate = true;
        }

        if (!string.IsNullOrEmpty(vm.LastName) && !vm.LastName.Equals(user.LastName))
        {
            user.LastName = vm.LastName;
            shouldUpdate = true;
        }

        if (ModelState.IsValid)
        {
            if (shouldUpdate)
            {
                await userStore.UpdateAsync(user, cancellationToken);
                ViewBag.StatusMessage = "Your profile has been updated";
            }

            await signInManager.RefreshSignInAsync(user);
        }

        return View(vm);
    }

    public async Task<IActionResult> Email()
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        var vm = new EmailViewModel()
        {
            Email = user.Email!,
            NewEmail = user.Email!,
            IsEmailConfirmed = await userManager.IsEmailConfirmedAsync(user)
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Email(EmailViewModel vm)
    {
        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var email = await userManager.GetEmailAsync(user);
        if (!string.IsNullOrEmpty(vm.NewEmail) && vm.NewEmail != email)
        {
            var userId = await userManager.GetUserIdAsync(user);
            var code = await userManager.GenerateChangeEmailTokenAsync(user, vm.NewEmail);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmailChange",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, email = vm.NewEmail, code = code },
                protocol: Request.Scheme);
            await emailSender.SendEmailAsync(
                vm.NewEmail,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            ViewBag.StatusMessage = "Confirmation link to change email sent. Please check your email.";
            //await userManager.SetEmailAsync(user, vm.NewEmail);
            return View(vm);
        }

        ViewBag.StatusMessage = "Your email is unchanged.";
        return View(vm);
    }

    public IActionResult ChangePassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            return View();
        }

        if (string.IsNullOrEmpty(vm.OldPassword) || string.IsNullOrEmpty(vm.NewPassword))
        {
            ViewBag.StatusMessage = "Must provide a new password to change password and also confirm it.";
            return View();
        }

        var changePasswordResult = await userManager.ChangePasswordAsync(user, vm.OldPassword, vm.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            foreach (var error in changePasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View();
        }

        await signInManager.RefreshSignInAsync(user);
        logger.LogInformation("User changed their password successfully.");
        ViewBag.StatusMessage = "Your password has been changed.";
        return View();
    }
}