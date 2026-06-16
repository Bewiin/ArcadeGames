using Microsoft.AspNetCore.Mvc;
using Projet_Groupe.Controllers;
using Projet_Groupe.Games;
using Xunit;

namespace Projet_Groupe.Tests;

public class GamesControllerTests
{
    [Fact]
    public void Index_RenvoieToutLeCatalogue()
    {
        var controller = new GamesController();

        var result = Assert.IsType<ViewResult>(controller.Index());

        Assert.Same(GamesCatalog.All, result.Model);
    }
}
