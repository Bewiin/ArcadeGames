using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projet_Groupe.Games;

namespace Projet_Groupe.Controllers;

[Authorize]
public class GamesController : Controller
{
    public IActionResult Index() => View(GamesCatalog.All);
}
