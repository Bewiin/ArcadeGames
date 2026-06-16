# Mastermind — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ajouter le jeu Mastermind comme mini-jeu solo dans l'Arcade Hub (ASP.NET Core MVC, thème néon).

**Architecture:** 1 Area `Mastermind` conforme à la convention du projet. Logique métier isolée dans `MastermindService` (fonctions pures, injectable). État de partie sérialisé en JSON dans la session HTTP. Pattern PRG (Post/Redirect/Get) sur toutes les actions POST.

**Tech Stack:** .NET 10, ASP.NET Core MVC, Identity (déjà configuré), xUnit (projet de test existant `tests/Projet-Groupe.Tests`), `System.Text.Json` (inclus).

---

## Structure des fichiers

| Fichier | Rôle |
|---|---|
| `Areas/Mastermind/Models/GameStatus.cs` | Enum : InProgress / Won / Lost |
| `Areas/Mastermind/Models/MastermindConstants.cs` | Couleurs, longueur du code, max essais |
| `Areas/Mastermind/Models/GuessResult.cs` | Un essai + ses pions (noirs/blancs) |
| `Areas/Mastermind/Models/MastermindState.cs` | État complet de partie (sérialisable JSON) |
| `Areas/Mastermind/Models/GuessViewModel.cs` | Données du formulaire POST |
| `Areas/Mastermind/Services/IMastermindService.cs` | Contrat du service |
| `Areas/Mastermind/Services/MastermindService.cs` | Logique pure : génération secret, évaluation, statut |
| `Areas/Mastermind/Controllers/MastermindController.cs` | Orchestration : session, service, PRG |
| `Areas/Mastermind/Views/_ViewImports.cshtml` | Using + TagHelpers pour l'Area |
| `Areas/Mastermind/Views/_ViewStart.cshtml` | Layout partagé |
| `Areas/Mastermind/Views/Mastermind/Index.cshtml` | Vue du jeu (formulaire + historique) |
| `tests/Projet-Groupe.Tests/MastermindServiceTests.cs` | Tests unitaires du service |
| `Projet-Groupe/Games/GamesCatalog.cs` | +1 ligne (Mastermind) |
| `Projet-Groupe/Program.cs` | +session + DI MastermindService |

---

### Task 1 : Constants & Models

**Files:**
- Create: `Projet-Groupe/Areas/Mastermind/Models/GameStatus.cs`
- Create: `Projet-Groupe/Areas/Mastermind/Models/MastermindConstants.cs`
- Create: `Projet-Groupe/Areas/Mastermind/Models/GuessResult.cs`
- Create: `Projet-Groupe/Areas/Mastermind/Models/MastermindState.cs`
- Create: `Projet-Groupe/Areas/Mastermind/Models/GuessViewModel.cs`

- [ ] **Step 1 : Créer l'enum GameStatus**

Create `Projet-Groupe/Areas/Mastermind/Models/GameStatus.cs` :
```csharp
namespace Projet_Groupe.Areas.Mastermind.Models;

public enum GameStatus { InProgress, Won, Lost }
```

- [ ] **Step 2 : Créer les constantes du jeu**

Create `Projet-Groupe/Areas/Mastermind/Models/MastermindConstants.cs` :
```csharp
namespace Projet_Groupe.Areas.Mastermind.Models;

public static class MastermindConstants
{
    public const int CodeLength = 4;
    public const int MaxAttempts = 10;

    public static readonly string[] AvailableColors =
        ["Rouge", "Jaune", "Vert", "Bleu", "Violet", "Orange"];

    public static readonly Dictionary<string, string> ColorEmojis = new()
    {
        ["Rouge"]  = "🔴",
        ["Jaune"]  = "🟡",
        ["Vert"]   = "🟢",
        ["Bleu"]   = "🔵",
        ["Violet"] = "🟣",
        ["Orange"] = "🟠",
    };
}
```

- [ ] **Step 3 : Créer GuessResult**

Create `Projet-Groupe/Areas/Mastermind/Models/GuessResult.cs` :
```csharp
namespace Projet_Groupe.Areas.Mastermind.Models;

public class GuessResult
{
    public string[] Guess     { get; set; } = [];
    public int      BlackPins { get; set; }
    public int      WhitePins { get; set; }
}
```

- [ ] **Step 4 : Créer MastermindState**

Create `Projet-Groupe/Areas/Mastermind/Models/MastermindState.cs` :
```csharp
namespace Projet_Groupe.Areas.Mastermind.Models;

public class MastermindState
{
    public string[]        Secret      { get; set; } = [];
    public List<GuessResult> History   { get; set; } = [];
    public GameStatus      Status      { get; set; } = GameStatus.InProgress;
    public int AttemptsLeft => MastermindConstants.MaxAttempts - History.Count;
}
```

- [ ] **Step 5 : Créer GuessViewModel**

Create `Projet-Groupe/Areas/Mastermind/Models/GuessViewModel.cs` :
```csharp
using System.ComponentModel.DataAnnotations;

namespace Projet_Groupe.Areas.Mastermind.Models;

public class GuessViewModel
{
    [Required] public string Color1 { get; set; } = "";
    [Required] public string Color2 { get; set; } = "";
    [Required] public string Color3 { get; set; } = "";
    [Required] public string Color4 { get; set; } = "";

    public string[] ToArray() => [Color1, Color2, Color3, Color4];
}
```

- [ ] **Step 6 : Compiler**

Run: `dotnet build Projet-Groupe/Projet-Groupe.csproj`
Expected: Build succeeded, 0 erreur.

- [ ] **Step 7 : Commit**

```bash
git add Projet-Groupe/Areas/Mastermind/Models
git commit -m "feat: ajoute les models du jeu Mastermind"
```

---

### Task 2 : Service (TDD)

**Files:**
- Create: `Projet-Groupe/Areas/Mastermind/Services/IMastermindService.cs`
- Create: `Projet-Groupe/Areas/Mastermind/Services/MastermindService.cs`
- Test:   `tests/Projet-Groupe.Tests/MastermindServiceTests.cs`

- [ ] **Step 1 : Écrire les tests qui échouent**

Create `tests/Projet-Groupe.Tests/MastermindServiceTests.cs` :
```csharp
using Projet_Groupe.Areas.Mastermind.Models;
using Projet_Groupe.Areas.Mastermind.Services;

namespace Projet_Groupe.Tests;

public class MastermindServiceTests
{
    private readonly MastermindService _sut = new();

    [Fact]
    public void Evaluate_CodeExact_Retourne4Noirs0Blancs()
    {
        string[] secret = ["Rouge", "Jaune", "Vert", "Bleu"];
        string[] guess  = ["Rouge", "Jaune", "Vert", "Bleu"];

        var result = _sut.Evaluate(secret, guess);

        Assert.Equal(4, result.BlackPins);
        Assert.Equal(0, result.WhitePins);
    }

    [Fact]
    public void Evaluate_AucuneCorrespondance_Retourne0Noirs0Blancs()
    {
        string[] secret = ["Rouge", "Rouge", "Rouge", "Rouge"];
        string[] guess  = ["Bleu",  "Bleu",  "Bleu",  "Bleu"];

        var result = _sut.Evaluate(secret, guess);

        Assert.Equal(0, result.BlackPins);
        Assert.Equal(0, result.WhitePins);
    }

    [Fact]
    public void Evaluate_CouleursBienMaisPositionsFausses_Retourne0Noirs4Blancs()
    {
        string[] secret = ["Rouge", "Jaune", "Vert",  "Bleu"];
        string[] guess  = ["Bleu",  "Vert",  "Jaune", "Rouge"];

        var result = _sut.Evaluate(secret, guess);

        Assert.Equal(0, result.BlackPins);
        Assert.Equal(4, result.WhitePins);
    }

    [Fact]
    public void Evaluate_CasMixte_RetourneBonsComptes()
    {
        // Rouge bien placé (noir), Jaune+Vert mal placés (blancs), Violet absent
        string[] secret = ["Rouge", "Jaune", "Vert",   "Bleu"];
        string[] guess  = ["Rouge", "Vert",  "Jaune",  "Violet"];

        var result = _sut.Evaluate(secret, guess);

        Assert.Equal(1, result.BlackPins);
        Assert.Equal(2, result.WhitePins);
    }

    [Fact]
    public void Evaluate_DoublonsDansSecret_ComptageCorrect()
    {
        // Secret: Rouge Rouge Jaune Vert — Guess: Rouge Bleu Rouge Orange
        // Noirs: position 0 (Rouge=Rouge) = 1
        // Restant secret [Rouge,Jaune,Vert], restant guess [Bleu,Rouge,Orange]
        // → Rouge trouvé 1 fois dans secret restant = 1 blanc
        string[] secret = ["Rouge", "Rouge", "Jaune",  "Vert"];
        string[] guess  = ["Rouge", "Bleu",  "Rouge",  "Orange"];

        var result = _sut.Evaluate(secret, guess);

        Assert.Equal(1, result.BlackPins);
        Assert.Equal(1, result.WhitePins);
    }

    [Fact]
    public void ResolveStatus_4Noirs_RetourneWon()
    {
        var state = new MastermindState
        {
            Secret  = ["Rouge", "Jaune", "Vert", "Bleu"],
            History = [new GuessResult { Guess = ["Rouge", "Jaune", "Vert", "Bleu"], BlackPins = 4, WhitePins = 0 }],
            Status  = GameStatus.InProgress,
        };

        var status = _sut.ResolveStatus(state);

        Assert.Equal(GameStatus.Won, status);
    }

    [Fact]
    public void ResolveStatus_HistoriquePlein_RetourneLost()
    {
        var history = Enumerable.Range(0, MastermindConstants.MaxAttempts)
            .Select(_ => new GuessResult { Guess = ["Bleu", "Bleu", "Bleu", "Bleu"], BlackPins = 0, WhitePins = 0 })
            .ToList();

        var state = new MastermindState
        {
            Secret  = ["Rouge", "Jaune", "Vert", "Orange"],
            History = history,
            Status  = GameStatus.InProgress,
        };

        var status = _sut.ResolveStatus(state);

        Assert.Equal(GameStatus.Lost, status);
    }

    [Fact]
    public void GenerateSecret_RetourneTableauDe4CouleursValides()
    {
        var secret = _sut.GenerateSecret();

        Assert.Equal(MastermindConstants.CodeLength, secret.Length);
        Assert.All(secret, c => Assert.Contains(c, MastermindConstants.AvailableColors));
    }
}
```

- [ ] **Step 2 : Lancer les tests pour vérifier qu'ils échouent**

Run: `dotnet test tests/Projet-Groupe.Tests`
Expected: FAIL (compilation — `MastermindService` introuvable).

- [ ] **Step 3 : Créer l'interface du service**

Create `Projet-Groupe/Areas/Mastermind/Services/IMastermindService.cs` :
```csharp
using Projet_Groupe.Areas.Mastermind.Models;

namespace Projet_Groupe.Areas.Mastermind.Services;

public interface IMastermindService
{
    string[]    GenerateSecret();
    GuessResult Evaluate(string[] secret, string[] guess);
    GameStatus  ResolveStatus(MastermindState state);
}
```

- [ ] **Step 4 : Implémenter le service**

Create `Projet-Groupe/Areas/Mastermind/Services/MastermindService.cs` :
```csharp
using Projet_Groupe.Areas.Mastermind.Models;

namespace Projet_Groupe.Areas.Mastermind.Services;

public class MastermindService : IMastermindService
{
    public string[] GenerateSecret() =>
        Enumerable.Range(0, MastermindConstants.CodeLength)
            .Select(_ => MastermindConstants.AvailableColors[
                Random.Shared.Next(MastermindConstants.AvailableColors.Length)])
            .ToArray();

    public GuessResult Evaluate(string[] secret, string[] guess)
    {
        var secretRemaining = new List<string>();
        var guessRemaining  = new List<string>();
        int blacks = 0;

        for (int i = 0; i < MastermindConstants.CodeLength; i++)
        {
            if (secret[i] == guess[i])
                blacks++;
            else
            {
                secretRemaining.Add(secret[i]);
                guessRemaining.Add(guess[i]);
            }
        }

        int whites = 0;
        foreach (var color in guessRemaining)
        {
            int idx = secretRemaining.IndexOf(color);
            if (idx < 0) continue;
            whites++;
            secretRemaining.RemoveAt(idx);
        }

        return new GuessResult { Guess = guess, BlackPins = blacks, WhitePins = whites };
    }

    public GameStatus ResolveStatus(MastermindState state)
    {
        var last = state.History.LastOrDefault();
        if (last?.BlackPins == MastermindConstants.CodeLength) return GameStatus.Won;
        if (state.History.Count >= MastermindConstants.MaxAttempts) return GameStatus.Lost;
        return GameStatus.InProgress;
    }
}
```

- [ ] **Step 5 : Lancer les tests pour vérifier qu'ils passent**

Run: `dotnet test tests/Projet-Groupe.Tests`
Expected: PASS — 12 tests (3 CatalogTests + 1 GamesControllerTests + 8 MastermindServiceTests).

- [ ] **Step 6 : Commit**

```bash
git add Projet-Groupe/Areas/Mastermind/Services tests/Projet-Groupe.Tests/MastermindServiceTests.cs
git commit -m "feat: ajoute le service Mastermind (TDD)"
```

---

### Task 3 : Controller + configuration session

**Files:**
- Create:  `Projet-Groupe/Areas/Mastermind/Controllers/MastermindController.cs`
- Modify:  `Projet-Groupe/Program.cs`

- [ ] **Step 1 : Créer le controller**

Create `Projet-Groupe/Areas/Mastermind/Controllers/MastermindController.cs` :
```csharp
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projet_Groupe.Areas.Mastermind.Models;
using Projet_Groupe.Areas.Mastermind.Services;

namespace Projet_Groupe.Areas.Mastermind.Controllers;

[Area("Mastermind")]
[Authorize]
public class MastermindController : Controller
{
    private const string SessionKey = "MastermindState";
    private readonly IMastermindService _service;

    public MastermindController(IMastermindService service) => _service = service;

    public IActionResult Index() => View(GetStateFromSession());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Start()
    {
        SaveStateToSession(new MastermindState { Secret = _service.GenerateSecret() });
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Guess(GuessViewModel vm)
    {
        var state = GetStateFromSession();
        if (state is null || state.Status != GameStatus.InProgress)
            return RedirectToAction(nameof(Index));

        var guess = vm.ToArray();
        if (!guess.All(c => MastermindConstants.AvailableColors.Contains(c)))
            return RedirectToAction(nameof(Index));

        var result = _service.Evaluate(state.Secret, guess);
        state.History.Add(result);
        state.Status = _service.ResolveStatus(state);
        SaveStateToSession(state);

        return RedirectToAction(nameof(Index));
    }

    private MastermindState? GetStateFromSession()
    {
        var json = HttpContext.Session.GetString(SessionKey);
        return json is null ? null : JsonSerializer.Deserialize<MastermindState>(json);
    }

    private void SaveStateToSession(MastermindState state) =>
        HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(state));
}
```

- [ ] **Step 2 : Ajouter la session et le DI dans Program.cs**

Dans `Projet-Groupe/Program.cs`, après la ligne `builder.Services.AddControllersWithViews();`, ajouter :
```csharp
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddScoped<IMastermindService, MastermindService>();
```

Ajouter l'using en tête du fichier :
```csharp
using Projet_Groupe.Areas.Mastermind.Services;
```

Et dans la section pipeline (après `app.UseRouting();`, avant `app.UseAuthorization();`) :
```csharp
app.UseSession();
```

- [ ] **Step 3 : Compiler**

Run: `dotnet build Projet-Groupe/Projet-Groupe.csproj`
Expected: Build succeeded, 0 erreur.

- [ ] **Step 4 : Commit**

```bash
git add Projet-Groupe/Areas/Mastermind/Controllers Projet-Groupe/Program.cs
git commit -m "feat: ajoute le controller Mastermind et configure la session"
```

---

### Task 4 : Views

**Files:**
- Create: `Projet-Groupe/Areas/Mastermind/Views/_ViewImports.cshtml`
- Create: `Projet-Groupe/Areas/Mastermind/Views/_ViewStart.cshtml`
- Create: `Projet-Groupe/Areas/Mastermind/Views/Mastermind/Index.cshtml`

- [ ] **Step 1 : Créer _ViewImports.cshtml**

Create `Projet-Groupe/Areas/Mastermind/Views/_ViewImports.cshtml` :
```cshtml
@using Projet_Groupe
@using Projet_Groupe.Areas.Mastermind.Models
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

- [ ] **Step 2 : Créer _ViewStart.cshtml**

Create `Projet-Groupe/Areas/Mastermind/Views/_ViewStart.cshtml` :
```cshtml
@{
    Layout = "_Layout";
}
```

- [ ] **Step 3 : Créer Index.cshtml**

Create `Projet-Groupe/Areas/Mastermind/Views/Mastermind/Index.cshtml` :
```cshtml
@model MastermindState?

@{
    ViewData["Title"] = "Mastermind";
}

<style>
    .mm-history { width: 100%; border-collapse: collapse; margin-bottom: 2rem; }
    .mm-history th, .mm-history td { padding: .6rem 1rem; text-align: center; border-bottom: 1px solid rgba(0,212,255,.15); }
    .mm-history th { color: var(--neon-blue); font-family: 'Orbitron', monospace; font-size: .75rem; letter-spacing: 1px; text-transform: uppercase; }
    .mm-emoji { font-size: 1.4rem; }
    .mm-pins  { font-size: 1.1rem; letter-spacing: 4px; }
    .mm-select { background: #0d0d1a; color: var(--text-primary); border: 1px solid rgba(0,212,255,.3); border-radius: 6px; padding: .4rem .6rem; font-family: 'Exo 2', sans-serif; font-size: 1rem; }
    .mm-select:focus { border-color: var(--neon-blue); outline: none; box-shadow: 0 0 6px rgba(0,212,255,.4); }
    .mm-alert { padding: 1rem 1.5rem; border-radius: 8px; font-family: 'Orbitron', monospace; font-weight: 700; letter-spacing: 1.5px; text-align: center; margin-bottom: 1.5rem; font-size: 1.1rem; }
    .mm-alert-win  { background: rgba(0,212,255,.1); border: 1px solid var(--neon-blue); color: var(--neon-blue); box-shadow: var(--glow-blue); }
    .mm-alert-lose { background: rgba(255,45,120,.1);  border: 1px solid var(--neon-pink); color: var(--neon-pink); box-shadow: var(--glow-pink); }
</style>

<div class="d-flex justify-content-between align-items-center mb-4 flex-wrap gap-3">
    <h1 class="neon-blue mb-0">🔐 MASTERMIND</h1>
    <form asp-area="Mastermind" asp-controller="Mastermind" asp-action="Start" method="post">
        <button type="submit" class="btn" style="background:linear-gradient(135deg,var(--neon-pink),var(--neon-purple));color:#fff;font-family:'Orbitron',monospace;font-size:.78rem;letter-spacing:1.5px;text-transform:uppercase;border:none;padding:.5rem 1.2rem;border-radius:6px;">
            NOUVELLE PARTIE
        </button>
    </form>
</div>

<hr class="neon-divider mb-4" />

@if (Model is null)
{
    <p class="text-center" style="color:var(--text-muted);font-family:'Exo 2',sans-serif;">
        Démarrez une nouvelle partie pour jouer.
    </p>
}
else
{
    @if (Model.Status == GameStatus.Won)
    {
        <div class="mm-alert mm-alert-win">
            GAGNÉ EN @Model.History.Count ESSAI@(Model.History.Count > 1 ? "S" : "") ! 🏆
        </div>
    }
    else if (Model.Status == GameStatus.Lost)
    {
        <div class="mm-alert mm-alert-lose">
            PERDU ! LE CODE ÉTAIT :
            @foreach (var c in Model.Secret)
            {
                <span class="mm-emoji">@MastermindConstants.ColorEmojis[c]</span>
            }
        </div>
    }
    else
    {
        <p style="color:var(--text-muted);font-family:'Exo 2',sans-serif;">
            Essais restants : <strong style="color:var(--neon-blue);">@Model.AttemptsLeft</strong> / @MastermindConstants.MaxAttempts
        </p>
    }

    @if (Model.History.Count > 0)
    {
        <table class="mm-history">
            <thead>
                <tr>
                    <th>#</th>
                    <th>Essai</th>
                    <th>⚫ Bien placés</th>
                    <th>⚪ Mal placés</th>
                </tr>
            </thead>
            <tbody>
                @for (int i = Model.History.Count - 1; i >= 0; i--)
                {
                    var r = Model.History[i];
                    <tr>
                        <td style="color:var(--text-muted)">@(i + 1)</td>
                        <td>
                            @foreach (var c in r.Guess)
                            {
                                <span class="mm-emoji">@MastermindConstants.ColorEmojis[c]</span>
                            }
                        </td>
                        <td>
                            <span class="mm-pins">@(new string('⚫', r.BlackPins))@(new string('⬛', MastermindConstants.CodeLength - r.BlackPins - r.WhitePins))</span>
                            <strong style="color:var(--neon-blue);">@r.BlackPins</strong>
                        </td>
                        <td>
                            <span class="mm-pins">@(new string('⚪', r.WhitePins))</span>
                            <strong style="color:var(--text-muted);">@r.WhitePins</strong>
                        </td>
                    </tr>
                }
            </tbody>
        </table>
    }

    @if (Model.Status == GameStatus.InProgress)
    {
        <form asp-area="Mastermind" asp-controller="Mastermind" asp-action="Guess" method="post"
              class="d-flex gap-3 align-items-center flex-wrap mt-3">
            @for (int i = 1; i <= MastermindConstants.CodeLength; i++)
            {
                <select name="Color@i" class="mm-select" required>
                    <option value="">— Couleur @i</option>
                    @foreach (var color in MastermindConstants.AvailableColors)
                    {
                        <option value="@color">@MastermindConstants.ColorEmojis[color] @color</option>
                    }
                </select>
            }
            <button type="submit" class="btn"
                    style="background:linear-gradient(135deg,var(--neon-blue),var(--neon-purple));color:#fff;font-family:'Orbitron',monospace;font-size:.78rem;letter-spacing:1.5px;text-transform:uppercase;border:none;padding:.5rem 1.4rem;border-radius:6px;">
                VALIDER
            </button>
        </form>
    }
}
```

- [ ] **Step 4 : Compiler**

Run: `dotnet build Projet-Groupe/Projet-Groupe.csproj`
Expected: Build succeeded, 0 erreur.

- [ ] **Step 5 : Commit**

```bash
git add Projet-Groupe/Areas/Mastermind/Views
git commit -m "feat: ajoute la vue Mastermind"
```

---

### Task 5 : Catalogue + vérification finale

**Files:**
- Modify: `Projet-Groupe/Games/GamesCatalog.cs`

- [ ] **Step 1 : Ajouter Mastermind au catalogue**

Dans `Projet-Groupe/Games/GamesCatalog.cs`, ajouter une entrée après `Demo` :
```csharp
new GameDescriptor(
    DisplayName: "Mastermind",
    Slug:        "mastermind",
    Description: "Déchiffre le code secret en 10 essais.",
    Icon:        "🔐",
    Area:        "Mastermind"),
```

- [ ] **Step 2 : Lancer tous les tests**

Run: `dotnet test tests/Projet-Groupe.Tests`
Expected: PASS — tous les tests existants + les nouveaux (slugs uniques inclus).

- [ ] **Step 3 : Vérifier manuellement**

Run: `dotnet run --project Projet-Groupe/Projet-Groupe.csproj`
Vérifier :
- `/Games` → carte Mastermind visible
- Clic sans login → redirige vers `/Identity/Account/Login`
- Après login → `/Mastermind/Mastermind` affiche l'écran de démarrage
- Bouton NOUVELLE PARTIE → formulaire de 4 selects
- Soumettre un essai → historique mis à jour avec pions
- 4 noirs → bannière victoire
- 10 essais sans succès → bannière défaite + révélation du code

- [ ] **Step 4 : Commit final**

```bash
git add Projet-Groupe/Games/GamesCatalog.cs
git commit -m "feat: enregistre Mastermind dans le catalogue des jeux"
```
