using System.Text;
using System.Text.Encodings.Web;
using Common.Constants;
using DapperIdentity.Models.Identity;
using DapperIdentity.Models.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

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

        var vm = new LoginViewModel()
        {
            ReturnUrl = returnUrl,
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
        };

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
    {
        vm.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (ModelState.IsValid)
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result =
                await _signInManager.PasswordSignInAsync(vm.Email, vm.Password, vm.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                if (string.IsNullOrEmpty(vm.ReturnUrl))
                {
                    return RedirectToAction("Index", "Home", new { area = "Guest" });
                }

                return LocalRedirect(vm.ReturnUrl);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(vm);
            }
        }

        ModelState.AddModelError(string.Empty, "Something went while trying to login.");

        // If we got this far, something failed, redisplay form
        return View(vm);
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
    public async Task<IActionResult> Register(RegisterViewModel vm, CancellationToken cancellationToken,
        string? returnUrl = null)
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

                var callbackUrl = Url.Action(
                    action: "ConfirmEmail",
                    controller: "Account",
                    values: new { area = "Identity", userId = userId, code = code },
                    protocol: Request.Scheme
                );

                // Just for testing coz email sender is not implemented yet.
                _logger.LogInformation(
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                await _emailSender.SendEmailAsync(vm.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.")
                    .ConfigureAwait(false);

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToAction("RegisterConfirmation", new { email = vm.Email, returnUrl = returnUrl });
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

    public async Task<IActionResult> RegisterConfirmation(string? email, string? returnUrl)
    {
        if (email is null)
        {
            return RedirectToAction("Index", "Home", new { area = "Guest" });
        }

        returnUrl = returnUrl ?? Url.Content("~/");

        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return NotFound($"Unable to load user with email '{email}'.");
        }

        var vm = new RegisterConfirmationViewModel
        {
            Email = email,
            // Once you add a real email sender, you should remove this code that lets you confirm the account
            DisplayConfirmAccountLink = true
        };

        if (vm.DisplayConfirmAccountLink)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            vm.EmailConfirmationUrl = Url.Action(
                action: "ConfirmEmail",
                controller: "Account",
                values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                protocol: Request.Scheme
            )!;
        }

        return View(vm);
    }

    public async Task<IActionResult> ConfirmEmail(string? userId, string? code)
    {
        if (userId is null || code is null)
        {
            return RedirectToAction("Index", "Home", new { area = "Guest" });
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound($"Unable to load user with ID '{userId}'.");
        }

        code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await _userManager.ConfirmEmailAsync(user, code);
        ViewBag.StatusMessage =
            result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
        return View();
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

    public IActionResult ForgotPassword()
    {
        var vm = new ForgotPasswordViewModel();
        return View(vm);
    }


    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(vm.Email);
            if (user is null /*|| !(await _userManager.IsEmailConfirmedAsync(user))*/)
            {
                // Don't reveal that the user does not exist or is not confirmed
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            // For more information on how to enable account confirmation and password reset please
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            _logger.LogInformation($"Reset Password Code Before Encoding ::::: {code}");

            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // This is for MVC
            var callbackUrl = Url.Action(
                action: "ResetPassword",
                controller: "Account",
                values: new { area = "Identity", code },
                protocol: Request.Scheme
            );

            await _emailSender.SendEmailAsync(
                vm.Email,
                "Reset Password",
                $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl!)}'>clicking here</a>.");

            _logger.LogInformation($"Reset Password Email ::: {HtmlEncoder.Default.Encode(callbackUrl)}");

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        return View(vm);
    }

    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    public IActionResult ResetPassword(string? code = null)
    {
        if (code is null)
        {
            return BadRequest("A code must be supplied for password reset.");
        }
        
        var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        _logger.LogInformation($"After Decoding::::{decodedCode}");

        var vm = new ResetPasswordViewModel()
        {
            Token = decodedCode
        };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var user = await _userManager.FindByEmailAsync(vm.Email);
        if (user is null)
        {
            // Don't reveal that the user does not exist
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        var result = await _userManager.ResetPasswordAsync(user, vm.Token, vm.Password);
        if (result.Succeeded)
        {
            return RedirectToAction(nameof(ResetPasswordConfirmation));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(vm);
    }

    public IActionResult ResetPasswordConfirmation()
    {
        return View();
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