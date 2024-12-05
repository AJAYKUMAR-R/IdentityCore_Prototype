using System.Diagnostics;
using IdentityNetCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityNetCore.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    //TO access this action role should have the department tech and role memeber
    //Which is configured in the middleware
    [Authorize(Policy = "MemberDep")]
    public IActionResult Member()
    {
        return View();
    }

    //TO access this action role should have the department tech and role Admin
    //Which is configured in the middleware
    [Authorize(Policy = "AdminDep")]
    public IActionResult Admin()
    {
        return View();
    }


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}