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
            DisplayName: "Stop The Wheel",
            Slug: "stopthewheel",
            Description: "Tirez le levier, arrêtez la roue et accumulez des points. Attention au crâne !",
            Icon: "🎡",
            Area: "Stopthewheel"),
        new GameDescriptor(
            DisplayName: "Snake Néon",
            Slug: "snake",
            Description: "Le classique revisité en néon. Mange, grandis, survive.",
            Icon: "🐍",
            Area: "Snake"),
        new GameDescriptor(
            DisplayName: "Tetris Néon",
            Slug: "tetris",
            Description: "Le Tetris classique en mode néon. Alignez les lignes, montez les niveaux.",
            Icon: "🟪",
            Area: "Tetris"),
    };
}
