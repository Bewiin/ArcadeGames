using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Projet_Groupe.Areas.NeonDodge.Controllers;

[Area("NeonDodge")]
[Authorize]
public class NeonDodgeController : Controller
{
    public IActionResult Index() => View();
}
