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
        new GameDescriptor(
            DisplayName: "Mastermind",
            Slug:        "mastermind",
            Description: "Déchiffre le code secret en 10 essais.",
            Icon:        "🔐",
            Area:        "Mastermind"),
    };
}
