# Architecture pluggable mini-jeux — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Livrer un squelette ASP.NET Core MVC permettant d'ajouter un mini-jeu en clonant une Area et en ajoutant une ligne au catalogue central.

**Architecture:** Catalogue statique (`GamesCatalog` → tableau de `GameDescriptor`). Un hub (`GamesController`) lit le catalogue et affiche une carte par jeu. Chaque jeu vit dans son propre Area (exemple `Demo`), protégé par `[Authorize]`. Convention : le nom du controller d'un jeu = le nom de son Area.

**Tech Stack:** .NET 10, ASP.NET Core MVC, Identity, xUnit (nouveau projet de test).

> **Dépendance à valider avant exécution :** ce plan crée un projet de test xUnit (packages `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk` — standard .NET, maintenus par Microsoft/équipe xUnit). À confirmer avant la Tâche 1.

---

## Structure des fichiers

| Fichier | Responsabilité |
|---|---|
| `Projet-Groupe/Games/GameDescriptor.cs` | Record immuable : donnée d'affichage d'un jeu |
| `Projet-Groupe/Games/GamesCatalog.cs` | Liste statique des jeux (seul fichier partagé à l'ajout) |
| `Projet-Groupe/Controllers/GamesController.cs` | Hub : transmet le catalogue à la vue |
| `Projet-Groupe/Views/Games/Index.cshtml` | Grille de cartes des jeux |
| `Projet-Groupe/Areas/Demo/Controllers/DemoController.cs` | Jeu exemple, `[Authorize]` |
| `Projet-Groupe/Areas/Demo/Views/Demo/Index.cshtml` | Page du jeu Demo |
| `Projet-Groupe/Areas/Demo/Views/_ViewImports.cshtml` | Tag helpers + using pour les vues de l'Area |
| `Projet-Groupe/Areas/Demo/Views/_ViewStart.cshtml` | Layout partagé pour les vues de l'Area |
| `Projet-Groupe/Program.cs` | Ajout de la route Areas |
| `Projet-Groupe/Views/Shared/_Layout.cshtml` | Lien « Jeux » dans la navbar |
| `tests/Projet-Groupe.Tests/Projet-Groupe.Tests.csproj` | Projet de test xUnit |
| `tests/Projet-Groupe.Tests/CatalogTests.cs` | Tests du catalogue |
| `tests/Projet-Groupe.Tests/GamesControllerTests.cs` | Test du hub |

---

### Task 1: Projet de test xUnit

**Files:**
- Create: `tests/Projet-Groupe.Tests/Projet-Groupe.Tests.csproj`

- [ ] **Step 1: Créer le projet de test et le référencement**

Run (depuis la racine du repo `C:\Users\Utilisateur\source\repos\Projet-Groupe`) :
```bash
dotnet new xunit -o tests/Projet-Groupe.Tests
dotnet add tests/Projet-Groupe.Tests/Projet-Groupe.Tests.csproj reference Projet-Groupe/Projet-Groupe.csproj
```

- [ ] **Step 2: Vérifier que la solution de test compile**

Run: `dotnet build tests/Projet-Groupe.Tests/Projet-Groupe.Tests.csproj`
Expected: Build succeeded.

- [ ] **Step 3: Supprimer le test généré par défaut**

Supprimer le fichier `tests/Projet-Groupe.Tests/UnitTest1.cs` (test vide du template).

- [ ] **Step 4: Commit**

```bash
git add tests/Projet-Groupe.Tests
git commit -m "test: ajoute le projet de test xunit"
```

---

### Task 2: GameDescriptor + GamesCatalog

**Files:**
- Create: `Projet-Groupe/Games/GameDescriptor.cs`
- Create: `Projet-Groupe/Games/GamesCatalog.cs`
- Test: `tests/Projet-Groupe.Tests/CatalogTests.cs`

- [ ] **Step 1: Écrire les tests qui échouent**

Create `tests/Projet-Groupe.Tests/CatalogTests.cs` :
```csharp
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
```

- [ ] **Step 2: Lancer les tests pour vérifier qu'ils échouent**

Run: `dotnet test tests/Projet-Groupe.Tests`
Expected: FAIL (compilation : `GamesCatalog` / `GameDescriptor` introuvables).

- [ ] **Step 3: Implémenter le descripteur**

Create `Projet-Groupe/Games/GameDescriptor.cs` :
```csharp
namespace Projet_Groupe.Games;

public record GameDescriptor(
    string DisplayName,
    string Slug,
    string Description,
    string Icon,
    string Area);
```

- [ ] **Step 4: Implémenter le catalogue**

Create `Projet-Groupe/Games/GamesCatalog.cs` :
```csharp
namespace Projet_Groupe.Games;

public static class GamesCatalog
{
    public static IReadOnlyList<GameDescriptor> All { get; } = new[]
    {
        new GameDescriptor(
            DisplayName: "Demo",
            Slug: "demo",
            Description: "Jeu exemple à cloner pour créer un nouveau jeu.",
            Icon: "🎮",
            Area: "Demo"),
    };
}
```

- [ ] **Step 5: Lancer les tests pour vérifier qu'ils passent**

Run: `dotnet test tests/Projet-Groupe.Tests`
Expected: PASS (3 tests).

- [ ] **Step 6: Commit**

```bash
git add Projet-Groupe/Games tests/Projet-Groupe.Tests/CatalogTests.cs
git commit -m "feat: ajoute le catalogue de jeux"
```

---

### Task 3: Hub GamesController + vue

**Files:**
- Create: `Projet-Groupe/Controllers/GamesController.cs`
- Create: `Projet-Groupe/Views/Games/Index.cshtml`
- Test: `tests/Projet-Groupe.Tests/GamesControllerTests.cs`

- [ ] **Step 1: Écrire le test qui échoue**

Create `tests/Projet-Groupe.Tests/GamesControllerTests.cs` :
```csharp
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
```

- [ ] **Step 2: Lancer le test pour vérifier qu'il échoue**

Run: `dotnet test tests/Projet-Groupe.Tests`
Expected: FAIL (compilation : `GamesController` introuvable).

- [ ] **Step 3: Implémenter le controller**

Create `Projet-Groupe/Controllers/GamesController.cs` :
```csharp
using Microsoft.AspNetCore.Mvc;
using Projet_Groupe.Games;

namespace Projet_Groupe.Controllers;

public class GamesController : Controller
{
    public IActionResult Index() => View(GamesCatalog.All);
}
```

- [ ] **Step 4: Lancer le test pour vérifier qu'il passe**

Run: `dotnet test tests/Projet-Groupe.Tests`
Expected: PASS.

- [ ] **Step 5: Créer la vue du hub**

Create `Projet-Groupe/Views/Games/Index.cshtml` :
```html
@model IReadOnlyList<Projet_Groupe.Games.GameDescriptor>
@{
    ViewData["Title"] = "Jeux";
}
<h1>Jeux</h1>
<div class="row">
    @foreach (var game in Model)
    {
        <div class="col-md-4 mb-4">
            <a class="text-decoration-none text-dark"
               asp-area="@game.Area" asp-controller="@game.Area" asp-action="Index">
                <div class="card h-100">
                    <div class="card-body">
                        <h5 class="card-title">@game.Icon @game.DisplayName</h5>
                        <p class="card-text">@game.Description</p>
                    </div>
                </div>
            </a>
        </div>
    }
</div>
```

- [ ] **Step 6: Commit**

```bash
git add Projet-Groupe/Controllers/GamesController.cs Projet-Groupe/Views/Games tests/Projet-Groupe.Tests/GamesControllerTests.cs
git commit -m "feat: ajoute le hub des jeux"
```

---

### Task 4: Area Demo + route Areas

**Files:**
- Create: `Projet-Groupe/Areas/Demo/Controllers/DemoController.cs`
- Create: `Projet-Groupe/Areas/Demo/Views/Demo/Index.cshtml`
- Create: `Projet-Groupe/Areas/Demo/Views/_ViewImports.cshtml`
- Create: `Projet-Groupe/Areas/Demo/Views/_ViewStart.cshtml`
- Modify: `Projet-Groupe/Program.cs` (ajout route Areas avant la route `default`)

- [ ] **Step 1: Créer le controller du jeu Demo**

Create `Projet-Groupe/Areas/Demo/Controllers/DemoController.cs` :
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Projet_Groupe.Areas.Demo.Controllers;

[Area("Demo")]
[Authorize]
public class DemoController : Controller
{
    public IActionResult Index() => View();
}
```

- [ ] **Step 2: Créer les fichiers de vue de l'Area**

Create `Projet-Groupe/Areas/Demo/Views/_ViewImports.cshtml` :
```cshtml
@using Projet_Groupe
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

Create `Projet-Groupe/Areas/Demo/Views/_ViewStart.cshtml` :
```cshtml
@{
    Layout = "_Layout";
}
```

Create `Projet-Groupe/Areas/Demo/Views/Demo/Index.cshtml` :
```cshtml
@{
    ViewData["Title"] = "Demo";
}
<h1>🎮 Jeu Demo</h1>
<p>Ici le jeu. Clone cette Area (<code>Areas/Demo</code>) pour créer un nouveau jeu, puis ajoute une ligne dans <code>GamesCatalog.All</code>.</p>
```

- [ ] **Step 3: Ajouter la route Areas dans Program.cs**

Dans `Projet-Groupe/Program.cs`, juste avant le bloc `app.MapControllerRoute(name: "default", ...)`, insérer :
```csharp
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
```

- [ ] **Step 4: Compiler et lancer les tests**

Run: `dotnet build Projet-Groupe/Projet-Groupe.csproj` puis `dotnet test tests/Projet-Groupe.Tests`
Expected: Build succeeded ; tous les tests PASS.

- [ ] **Step 5: Commit**

```bash
git add Projet-Groupe/Areas/Demo Projet-Groupe/Program.cs
git commit -m "feat: ajoute l'area demo et la route des areas"
```

---

### Task 5: Lien « Jeux » dans la navbar

**Files:**
- Modify: `Projet-Groupe/Views/Shared/_Layout.cshtml` (navbar, après le `<li>` Privacy, vers la ligne 28)

- [ ] **Step 1: Ajouter l'élément de navigation**

Dans `Projet-Groupe/Views/Shared/_Layout.cshtml`, après le `<li class="nav-item">` contenant le lien Privacy, ajouter :
```html
                        <li class="nav-item">
                            <a class="nav-link text-dark" asp-area="" asp-controller="Games" asp-action="Index">Jeux</a>
                        </li>
```

- [ ] **Step 2: Vérifier manuellement le rendu**

Run: `dotnet run --project Projet-Groupe/Projet-Groupe.csproj`
Expected : la navbar affiche « Jeux » ; `/Games` liste la carte Demo ; cliquer dessus redirige vers la page de connexion (non authentifié), puis affiche la page Demo une fois connecté.

- [ ] **Step 3: Commit**

```bash
git add Projet-Groupe/Views/Shared/_Layout.cshtml
git commit -m "feat: ajoute le lien jeux dans la navbar"
```

---

## Procédure d'ajout d'un nouveau jeu (référence)

1. Copier `Projet-Groupe/Areas/Demo` → `Projet-Groupe/Areas/<MonJeu>`.
2. Renommer `DemoController` → `<MonJeu>Controller`, l'attribut `[Area("<MonJeu>")]`, le namespace, et le dossier `Views/Demo` → `Views/<MonJeu>`.
3. Ajouter une ligne dans `GamesCatalog.All` (convention : `Area` = nom du controller).
