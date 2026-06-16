using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Projet_Groupe.Areas.Tetris.Controllers;

[Area("Tetris")]
[Authorize]
public class TetrisController : Controller
{
    public IActionResult Index() => View();
}
