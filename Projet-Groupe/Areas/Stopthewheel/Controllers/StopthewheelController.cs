using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Projet_Groupe.Areas.Stopthewheel.Controllers;

[Area("Stopthewheel")]
[Authorize]
public class StopthewheelController : Controller
{
    public IActionResult Index() => View();
}
