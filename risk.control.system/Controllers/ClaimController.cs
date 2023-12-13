using Microsoft.AspNetCore.Mvc;

namespace risk.web.MVC.Controllers
{
public class ClaimController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
}