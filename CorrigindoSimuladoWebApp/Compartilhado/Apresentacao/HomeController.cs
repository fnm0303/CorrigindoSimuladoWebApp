using Microsoft.AspNetCore.Mvc;

public sealed class HomeController : Controller
{
    [HttpGet]
    public ActionResult Index()
    {
        return View();
    }
}