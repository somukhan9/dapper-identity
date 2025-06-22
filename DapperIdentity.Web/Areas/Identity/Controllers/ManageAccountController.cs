using DapperIdentity.Models.Identity;
using DapperIdentity.Models.ViewModels.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DapperIdentity.Web.Areas.Identity.Controllers;

[Area(areaName: "Identity")]
public class ManageAccountController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IUserStore<ApplicationUser> userStore)
    : Controller
{
    public async Task<IActionResult> Index()
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        var manageAccVM = await LoadAsync(user);
        return View("Index", manageAccVM);
    }

    [HttpPost]
    public async Task<IActionResult> Index(ManageAccountViewModel manageAccVM, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            manageAccVM = await LoadAsync(user);
            return View("Index", manageAccVM);
        }

        var shouldUpdate = false;

        if (!string.IsNullOrEmpty(manageAccVM.PhoneNumber) && !manageAccVM.PhoneNumber.Equals(user.PhoneNumber))
        {
            user.PhoneNumber = manageAccVM.PhoneNumber;
            shouldUpdate = true;
        }

        if (!string.IsNullOrEmpty(manageAccVM.FirstName) && !manageAccVM.FirstName.Equals(user.FirstName))
        {
            user.FirstName = manageAccVM.FirstName;
            shouldUpdate = true;
        }

        if (!string.IsNullOrEmpty(manageAccVM.LastName) && !manageAccVM.LastName.Equals(user.LastName))
        {
            user.LastName = manageAccVM.LastName;
            shouldUpdate = true;
        }

        if (shouldUpdate)
        {
            await userStore.UpdateAsync(user, cancellationToken);
        }

        await signInManager.RefreshSignInAsync(user);
        manageAccVM.StatusMessage = "Your profile has been updated";
        return RedirectToAction(nameof(Index));
    }

    #region Private Methods Specific to Controller
    private async Task<ManageAccountViewModel> LoadAsync(ApplicationUser user)
    {
        var manageAccVM = new ManageAccountViewModel() { Username = user.UserName!, PhoneNumber = user.PhoneNumber, FirstName = user.FirstName, LastName = user.LastName };

        return manageAccVM;
    }
    #endregion
}