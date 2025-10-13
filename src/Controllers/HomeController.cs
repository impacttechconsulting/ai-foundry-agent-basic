using Microsoft.AspNetCore.Mvc;

namespace AiFoundryAgent.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}