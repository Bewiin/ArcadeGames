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
