using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Projet_Groupe.Areas.Demo.Controllers;

[Area("Demo")]
[Authorize]
public class DemoController : Controller
{
    public IActionResult Index() => View();
}
