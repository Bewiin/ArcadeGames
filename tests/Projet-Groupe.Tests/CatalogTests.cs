using Projet_Groupe.Games;
using Xunit;

namespace Projet_Groupe.Tests;

public class CatalogTests
{
    [Fact]
    public void All_NEstPasVide()
    {
        Assert.NotEmpty(GamesCatalog.All);
    }

    [Fact]
    public void All_ContientLeJeuDemo()
    {
        Assert.Contains(GamesCatalog.All, g => g.Slug == "demo");
    }

    [Fact]
    public void All_SlugsSontUniques()
    {
        var slugs = GamesCatalog.All.Select(g => g.Slug).ToList();
        Assert.Equal(slugs.Count, slugs.Distinct().Count());
    }
}
