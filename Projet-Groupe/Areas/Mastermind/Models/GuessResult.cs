namespace Projet_Groupe.Areas.Mastermind.Models;

public class GuessResult
{
    public string[] Guess     { get; init; } = [];
    public int      BlackPins { get; init; }
    public int      WhitePins { get; init; }
}
