using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Projet_Groupe.Areas.Snake.Controllers;

[Area("Snake")]
[Authorize]
public class SnakeController : Controller
{
    public IActionResult Index() => View();
}
