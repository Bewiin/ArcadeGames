# Ajouter un mini-jeu

Architecture pluggable : chaque jeu est isolé dans son propre **Area**. Le seul
fichier partagé à modifier est le catalogue (`Games/GamesCatalog.cs`) — donc quasi
aucun conflit de merge quand on bosse à plusieurs.

## Stack

- ASP.NET Core MVC, .NET 10, Identity (login requis), SQL Server
- Tests : xUnit (`tests/Projet-Groupe.Tests`) → `dotnet test`
- App en dev : `http://localhost:5025` (`dotnet run --project Projet-Groupe`)

## Comment ça marche

- `Games/GameDescriptor.cs` : description d'un jeu (nom, slug, description, icône, Area).
- `Games/GamesCatalog.cs` : liste statique de tous les jeux.
- `Controllers/GamesController.cs` + `Views/Games/Index.cshtml` : le hub `/Games`
  affiche une carte par jeu présent dans le catalogue.
- Chaque jeu vit dans `Areas/<NomDuJeu>/`, son controller porte `[Authorize]`
  (accès réservé aux utilisateurs connectés).

**Convention importante :** le nom du controller d'un jeu = le nom de son Area.
Le hub génère le lien avec `asp-controller="@game.Area"`.

## Étapes pour ajouter un jeu (exemple : "Snake")

1. **Cloner l'Area exemple** `Areas/Demo` → `Areas/Snake`.

2. **Renommer dans la copie :**
   - `DemoController.cs` → `SnakeController.cs`
   - la classe `DemoController` → `SnakeController`
   - l'attribut `[Area("Demo")]` → `[Area("Snake")]`
   - le namespace `Projet_Groupe.Areas.Demo.Controllers` → `...Areas.Snake.Controllers`
   - le dossier de vues `Views/Demo` → `Views/Snake`

3. **Ajouter une ligne dans `Games/GamesCatalog.cs`** :

   ```csharp
   new GameDescriptor(
       DisplayName: "Snake",
       Slug: "snake",
       Description: "Le serpent classique.",
       Icon: "🐍",
       Area: "Snake"),
   ```

4. **Vérifier** : `dotnet test` (les slugs doivent rester uniques), puis
   `dotnet run` → le jeu apparaît sur `/Games` et son lien mène vers `/Snake/Snake`.

C'est tout. La logique du jeu se code dans le controller `Snake` et ses vues.
