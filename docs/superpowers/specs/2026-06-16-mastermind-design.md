# Design — Mastermind

Date : 2026-06-16
Projet : Projet-Groupe (ASP.NET Core MVC, .NET 10, Identity, SQL Server)
Branche : feat/mastermind

## Objectif

Ajouter le jeu Mastermind comme mini-jeu solo dans l'Arcade Hub. Le joueur doit
deviner un code secret de 4 couleurs en 10 essais maximum. Après chaque essai,
le serveur retourne des indices (pions noirs = bonne couleur + bonne position,
pions blancs = bonne couleur + mauvaise position). Accès protégé par login.

## Règles du jeu

- 6 couleurs disponibles : Rouge, Jaune, Vert, Bleu, Violet, Orange
- Code secret : 4 couleurs, tirées aléatoirement (doublons possibles)
- 10 essais maximum
- Pion noir : couleur et position correctes
- Pion blanc : couleur correcte, position incorrecte
- Partie gagnée : 4 pions noirs
- Partie perdue : 10 essais épuisés sans trouver le code

## Architecture

Un jeu = une Area. Suit exactement la convention du projet (`Areas/Mastermind`).
Le service est enregistré en DI (`AddScoped`) dans `Program.cs`.
L'état de partie est sérialisé en JSON dans la session HTTP.

## Structure des fichiers

```
Areas/Mastermind/
├── Controllers/
│   └── MastermindController.cs
├── Models/
│   ├── MastermindState.cs
│   ├── GuessResult.cs
│   └── GuessViewModel.cs
├── Services/
│   ├── IMastermindService.cs
│   └── MastermindService.cs
└── Views/
    ├── _ViewImports.cshtml
    ├── _ViewStart.cshtml
    └── Mastermind/
        └── Index.cshtml

Games/GamesCatalog.cs              (+1 ligne)
Program.cs                         (enregistrement DI du service)
```

## Models

```csharp
public enum GameStatus { InProgress, Won, Lost }

public class GuessResult
{
    public string[] Guess { get; init; }
    public int BlackPins { get; init; }
    public int WhitePins { get; init; }
}

public class MastermindState
{
    public string[] Secret { get; init; }
    public List<GuessResult> History { get; init; } = [];
    public GameStatus Status { get; set; } = GameStatus.InProgress;
    public int MaxAttempts => 10;
    public int AttemptsLeft => MaxAttempts - History.Count;
}

public class GuessViewModel
{
    [Required] public string Color1 { get; set; }
    [Required] public string Color2 { get; set; }
    [Required] public string Color3 { get; set; }
    [Required] public string Color4 { get; set; }
}
```

## Service

```csharp
public interface IMastermindService
{
    string[] GenerateSecret();
    GuessResult Evaluate(string[] secret, string[] guess);
    GameStatus ResolveStatus(MastermindState state);
}
```

### Algorithme Evaluate (pions noirs/blancs)

1. Calculer les noirs : positions identiques entre secret et guess.
2. Pour les positions restantes (non noires), compter les couleurs en commun
   (min des occurrences côté secret vs côté guess) → blancs.
3. Les doublons sont gérés par comptage de fréquences, pas par simple égalité.

## Controller

Actions :
- `GET  Index`  : lit la session → affiche la partie en cours ou l'écran de démarrage
- `POST Start`  : crée un nouveau `MastermindState`, écrit en session, redirect Index
- `POST Guess`  : lit session, valide `GuessViewModel`, appelle le service, met à jour
                  l'état, écrit en session, redirect Index (PRG pattern)

Le controller ne contient aucune logique de jeu : il orchestre uniquement.

## Vue (Index.cshtml)

Trois zones :
1. En-tête : titre + bouton "NOUVELLE PARTIE" (POST Start)
2. Historique : tableau des essais passés avec couleurs + pions
3. Formulaire : 4 `<select>` (couleurs en emoji) + bouton "VALIDER" — masqué si partie terminée

Rendu conditionnel :
- Partie gagnée → bandeau vert néon + nombre d'essais utilisés
- Partie perdue → bandeau rose néon + révélation du code secret
- Conforme à la charte graphique (variables CSS néon, Orbitron/Exo2, fond sombre)

## Tests (xUnit, projet existant)

```
MastermindServiceTests
├── Evaluate_CodeExact_Retourne4Noirs0Blancs
├── Evaluate_AucuneCorrespondance_Retourne0Noirs0Blancs
├── Evaluate_CouleursBienMaisPositionsFausses_Retourne0Noirs4Blancs
├── Evaluate_CasMixte_RetourneBonsComptes
├── Evaluate_DoublonsDansSecret_ComptageCorrect
├── ResolveStatus_4Noirs_RetourneWon
└── ResolveStatus_HistoriquePlein_RetourneLost
```

## Hors périmètre

- Persistance des scores en base (fonctionnalité future commune)
- Mode multijoueur
- Difficulté configurable (longueur du code, nombre de couleurs)
