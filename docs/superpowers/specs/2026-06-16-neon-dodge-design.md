# Design — Neon Dodge

Date : 2026-06-16
Projet : Projet-Groupe (ASP.NET Core MVC, .NET 10, Identity)
Branche : feat/neon-dodge

## Objectif

Ajouter le jeu arcade "Neon Dodge" : route top-down, voiture du joueur contrôlée aux flèches, obstacles variés à éviter, 3 vies, score croissant avec le temps, musique chiptune générée via Web Audio API. Jeu purement client-side (JS Canvas), l'Area MVC sert uniquement de wrapper avec authentification.

## Architecture

Un jeu = une Area. Zéro logique serveur — toute la logique de jeu est en JavaScript dans la vue. Le controller expose uniquement `GET Index()` avec `[Authorize]`.

```
Areas/NeonDodge/
├── Controllers/NeonDodgeController.cs
├── Views/
│   ├── _ViewImports.cshtml
│   ├── _ViewStart.cshtml
│   └── NeonDodge/Index.cshtml      ← Canvas + JS complet
Games/GamesCatalog.cs               ← +1 ligne
```

## Controller

```csharp
[Area("NeonDodge")]
[Authorize]
public class NeonDodgeController : Controller
{
    public IActionResult Index() => View();
}
```

Aucun service, aucune session, aucune dépendance DI.

## Vue — Canvas

**Dimensions canvas :** 400×600px, centré dans la page.

### Route
- Fond : `#07070f`
- 3 voies séparées par des lignes pointillées blanches animées (défilement vers le bas → illusion de vitesse)
- Bandes latérales en `--neon-pink`

### Voiture joueur
- Rectangle dessiné en Canvas (pas d'image externe)
- Couleur `--neon-blue` + glow bleu
- Position Y fixe (bas du canvas), déplacement X aux flèches gauche/droite
- Largeur : 40px, hauteur : 60px

### Obstacles (3 types)
| Type | Couleur | Largeur | Hauteur | Comportement |
|---|---|---|---|---|
| Voiture (`CAR`) | Rouge néon | 40px | 60px | Ligne droite, vitesse standard |
| Camion (`TRUCK`) | Violet néon | 80px | 90px | Ligne droite, vitesse lente |
| Baril (`BARREL`) | Orange néon | 30px | 30px | Zigzag (±1px/frame), vitesse rapide |

### HUD (dessiné sur le canvas)
```
❤️❤️❤️    [SCORE : 1240]    [🔊]
```
- Vies à gauche, score centré, bouton mute à droite
- Police : `Orbitron` (chargée via Google Fonts dans le layout)

### Écrans
- **Start** : overlay sombre + "APPUIE SUR ESPACE" en neon-flicker
- **Game over** : score final + meilleur score (localStorage) + "REJOUER — ESPACE"
- **En jeu** : canvas uniquement, HUD intégré

## Mécanique de jeu

### Game loop (requestAnimationFrame, ~60fps)
1. Mise à jour position joueur (input clavier)
2. Spawn obstacles (timer + probabilité selon niveau)
3. Déplacement obstacles vers le bas
4. Détection collision AABB (joueur vs chaque obstacle)
5. Mise à jour score
6. Rendu canvas

### Difficulté progressive
- Toutes les 10 secondes : `vitesse_base × 1.1`, fréquence spawn +10%
- Pas de cap — le jeu devient graduellement injouable (arcade pur)

### Collision
- AABB standard (rectangles)
- À la collision : vie -1, flash rouge 500ms, invincibilité 2s
- 0 vies → game over

### Score
- `Math.floor(tempsEnSecondes × 10)` mis à jour chaque seconde
- Meilleur score sauvegardé en `localStorage` (clé `neonDodgeBestScore`)

## Musique (Web Audio API)

Boucle de 8 mesures, style chiptune, générée entièrement en JS :
- `OscillatorNode` (onde carrée) pour la mélodie
- `GainNode` pour l'enveloppe
- Séquence de notes codée en tableau de fréquences
- Bouton `M` (clavier) ou clic icône HUD pour mute/unmute
- Démarre à l'entrée dans l'écran de jeu (après interaction utilisateur — contrainte navigateur)

## Contrôles
| Touche | Action |
|---|---|
| ⬅️ | Déplacer à gauche |
| ➡️ | Déplacer à droite |
| Espace | Démarrer / Rejouer |
| M | Mute / Unmute |

## Catalogue
```csharp
new GameDescriptor(
    DisplayName: "Neon Dodge",
    Slug:        "neondodge",
    Description: "Évite les voitures, camions et barils sur la route néon.",
    Icon:        "🚗",
    Area:        "NeonDodge"),
```

## Tests
Aucun test unitaire côté serveur (le controller est trivial). Le test de slug unique dans `CatalogTests` couvre l'intégration catalogue.

## Hors périmètre
- Leaderboard persistant (base de données)
- Niveaux prédéfinis
- Mobile / touch controls
