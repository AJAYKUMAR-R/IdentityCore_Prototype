using System;
using System.Diagnostics;
using IdentityNetCore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace IdentityNetCore.Controllers;

public class Person
{
    public string UserName { get; set; }
}

public class AttackController() : Controller
{
    //This is going to consume the API from the other domain : https://localhost:51446/Identity/ReturnCROSUIS which is from the app CookieBased
    //To Check agains the cross issue i have reading the other domain data using jquery ajax call
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    //In case if some one stole the cookie to access our application
    //This will validate antiforger token inside the form with cookie to prevent XSRF attack
    [ValidateAntiForgeryToken]
    [HttpPost]
    public IActionResult Index(Person person)
    {
        return View("Result",person);
    }

    [HttpPost]
    public IActionResult RedirectAttack()
    {
        return LocalRedirect("index"); //This wil check the whether the redirect url is in our application or not before redirecting it
    }

}
