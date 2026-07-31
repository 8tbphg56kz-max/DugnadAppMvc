using Microsoft.AspNetCore.Mvc;

namespace DugnadApp.Controllers
{
    public class InformationController : Controller
    {
        public IActionResult Om()
        {
            return View();
        }

        public IActionResult Personvern()
        {
            return View();
        }
    }
}