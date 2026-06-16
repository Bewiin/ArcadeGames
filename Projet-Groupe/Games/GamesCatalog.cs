namespace Projet_Groupe.Games;

public static class GamesCatalog
{
    public static IReadOnlyList<GameDescriptor> All { get; } = new[]
    {
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
            Slug:        "tetris",
            Description: "Le Tetris classique en mode néon. Alignez les lignes, montez les niveaux.",
            Icon:        "🟪",
            Area:        "Tetris"),
        new GameDescriptor(
            DisplayName: "Mastermind",
            Slug:        "mastermind",
            Description: "Déchiffre le code secret en 10 essais.",
            Icon:        "🔐",
            Area:        "Mastermind"),
        new GameDescriptor(
            DisplayName: "Neon Dodge",
            Slug:        "neondodge",
            Description: "Évite les voitures, camions et barils sur la route néon.",
            Icon:        "🚗",
            Area:        "NeonDodge"),
    };
}
