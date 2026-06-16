using Microsoft.AspNetCore.Mvc;
using Projet_Groupe.Games;

namespace Projet_Groupe.Controllers;

public class GamesController : Controller
{
    public IActionResult Index() => View(GamesCatalog.All);
}
