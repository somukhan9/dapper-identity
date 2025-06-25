using Common.Constants;
using DapperIdentity.Models.Identity;
using DapperIdentity.Models.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

namespace DapperIdentity.Web.Areas.Identity.Controllers;

[Area(areaName: "Identity")]
public class AccountController : Controller
{
    private readonly ILogger<AccountController> _logger;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IUserEmailStore<ApplicationUser> _emailStore;
    private readonly IRoleStore<ApplicationRole> _roleStore;
    private readonly IEmailSender _emailSender;

    public AccountController(ILogger<AccountController> logger,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        IEmailSender emailSender,
        IRoleStore<ApplicationRole> roleStore)
    {
        _logger = logger;
        _signInManager = signInManager;
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _roleStore = roleStore;
        _emailSender = emailSender;
    }

    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        var loginVM = new LoginViewModel()
        {
            ReturnUrl = returnUrl,
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
        };

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        return View(loginVM);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel loginVM, string? returnUrl = null)
    {
        loginVM.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (ModelState.IsValid)
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(loginVM.Email, loginVM.Password, loginVM.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                if (string.IsNullOrEmpty(loginVM.ReturnUrl))
                {
                    return RedirectToAction("Index", "Home", new { area = "Guest" });
                }

                return LocalRedirect(loginVM.ReturnUrl);
            }
            /*if (result.RequiresTwoFactor)
            {
                return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = loginVM.RememberMe });
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return RedirectToPage("./Lockout");
            }*/
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(loginVM);
            }
        }

        ModelState.AddModelError(string.Empty, "Something went while trying to login.");

        // If we got this far, something failed, redisplay form
        return View(loginVM);
    }

    public async Task<IActionResult> Register(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        var vm = new RegisterViewModel
        {
            ReturnUrl = returnUrl,
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel vm, CancellationToken cancellationToken, string? returnUrl = null)
    {
        // Create Admin Role if does not exist
        var roleAdmin = await _roleStore.FindByNameAsync(SD.ROLE_ADMIN, cancellationToken);
        if (roleAdmin is null)
            await _roleStore.CreateAsync(new ApplicationRole()
            {
                Name = SD.ROLE_ADMIN
            }, cancellationToken);

        // Create Guest Role if does not exist
        var roleGuest = await _roleStore.FindByNameAsync(SD.ROLE_GUEST, cancellationToken);
        if (roleGuest is null)
            await _roleStore.CreateAsync(new ApplicationRole()
            {
                Name = SD.ROLE_GUEST
            }, cancellationToken);

        // Actual registration process starts from here
        vm.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        if (ModelState.IsValid)
        {
            var user = CreateUser();

            await _userStore.SetUserNameAsync(user, vm.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, vm.Email, CancellationToken.None);

            var result = await _userManager.CreateAsync(user, vm.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                // Assign role to user
                if (string.IsNullOrEmpty(vm.Role))
                    await _userManager.AddToRoleAsync(user, SD.ROLE_GUEST);
                else
                    await _userManager.AddToRoleAsync(user, vm.Role);

                _logger.LogInformation($"Successfully assigned role to user {user.UserName}.");

                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Identity/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code },
                    protocol: Request.Scheme);

                // Just for testing coz email sender is not implemented yet.
                _logger.LogInformation($"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                await _emailSender.SendEmailAsync(vm.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    //return RedirectToAction("RegisterConfirmation", new { email = vm.Email, returnUrl = returnUrl });
                    //return RedirectToPage("RegisterConfirmation", new { email = vm.Email, returnUrl = returnUrl });
                }
                else
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    if (string.IsNullOrEmpty(vm.ReturnUrl))
                    {
                        return RedirectToAction("Index", "Home", new { area = "Guest" });
                    }

                    return LocalRedirect(vm.ReturnUrl);
                }
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View("Register", vm);
        }

        ModelState.AddModelError(string.Empty, "Something went wrong.");
        // If we got this far, something failed, redisplay form
        return View("Register", vm);
    }

    public async Task<IActionResult> Logout(string? returnUrl = null)
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");

        if (string.IsNullOrEmpty(returnUrl))
        {
            return RedirectToAction("Index", "Home", new { area = "Guest" });
        }
        return LocalRedirect(returnUrl);
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    #region Controller Specific Private Methods
    private ApplicationUser CreateUser()
    {
        try
        {
            return Activator.CreateInstance<ApplicationUser>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                                                $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                                                $"override the register page in /Areas/Identity/Views/Account/Register.cshtml");
        }
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<ApplicationUser>)_userStore;
    }
    #endregion
}