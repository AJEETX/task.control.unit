using Microsoft.AspNetCore.Mvc;

namespace risk.web.MVC.Controllers
{
public class UnderwritingController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
}