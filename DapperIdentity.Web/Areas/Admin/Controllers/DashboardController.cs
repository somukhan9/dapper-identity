using Common.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DapperIdentity.Web.Areas.Admin.Controllers;

[Area(areaName: "Admin")]
[Authorize(Roles = SD.ROLE_ADMIN)]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}