# Design — Architecture pluggable « mini-jeux »

Date : 2026-06-16
Projet : Projet-Groupe (ASP.NET Core MVC, .NET 10, Identity, SQL Server)

## Objectif

Fournir un squelette permettant d'ajouter facilement de nouveaux mini-jeux,
chaque jeu étant développable indépendamment par un membre de l'équipe (duo)
avec un minimum de fichiers partagés. Livraison initiale : le câblage + un jeu
exemple clonable. Accès aux jeux protégé par authentification (exigence du brief).

## Décisions verrouillées

- Catalogue **central explicite** (liste statique), pas de découverte par réflexion.
- **Un jeu = un Area** (isolation forte, routing propre).
- Livraison initiale = **squelette minimal** : catalogue + hub + 1 jeu exemple `Demo`.
  Pas de persistance des scores pour l'instant (ajoutée plus tard si un jeu en a besoin).

## Briques

### 1. `GameDescriptor` (donnée pure)
Record immuable décrivant un jeu pour l'affichage du hub :
- `DisplayName` : nom affiché
- `Slug` : identifiant court unique
- `Description` : texte court
- `Icon` : emoji / classe d'icône
- `Area` : nom de l'Area du jeu (cible du lien)

Aucune logique, uniquement de la donnée d'affichage.

### 2. `GamesCatalog` (catalogue central)
Classe statique exposant `IReadOnlyList<GameDescriptor> All` — un simple tableau
de descripteurs. **Unique fichier partagé** modifié à chaque ajout de jeu (1 ligne).
Choix KISS : tableau de constantes plutôt que Registry/Factory, le besoin étant statique.

### 3. Hub — `GamesController`
`Index()` lit `GamesCatalog.All` et passe la liste à la vue. La vue affiche une
carte cliquable par jeu (icône, titre, description) liée à l'Area du jeu.
Page d'accueil des jeux, accessible depuis la navbar. Aucune logique métier dans
le controller : il ne fait que transmettre le catalogue.

### 4. Un jeu = un Area (exemple `Demo`)
```
Areas/Demo/Controllers/DemoController.cs   [Authorize] → Index()
Areas/Demo/Views/Demo/Index.cshtml         page "ici le jeu Demo"
```
`[Authorize]` sur le controller → accès protégé par login. Dossier à cloner pour
chaque nouveau jeu.

### 5. Câblage
- `Program.cs` : route Areas `{area:exists}/{controller=Home}/{action=Index}/{id?}`.
- `_Layout.cshtml` : lien « Jeux » vers le hub dans la navbar.

## Ajouter un jeu (procédure)
1. Cloner `Areas/Demo` → `Areas/<MonJeu>` ; renommer controller + vues + namespace.
2. Ajouter une ligne dans `GamesCatalog.All`.

## Flux
Hub lit le catalogue (statique) → vue rend les cartes → clic → controller de
l'Area rend la vue du jeu.

## Tests
- Cas nominal : `GamesController.Index` renvoie tous les descripteurs du catalogue.
- Cas limite : tous les `Slug` du catalogue sont uniques (évite les doublons lors
  d'ajouts en parallèle).

## Hors périmètre (pour plus tard)
- Persistance des scores (table liée à l'utilisateur Identity + service commun).
- Découverte automatique des jeux par réflexion.
