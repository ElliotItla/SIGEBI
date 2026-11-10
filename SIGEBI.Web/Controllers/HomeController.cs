using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SIGEBI.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    [AllowAnonymous]
    public IActionResult Error()
    {
        return View();
    }

    public IActionResult AccesoDenegado()
    {
        return View();
    }
}