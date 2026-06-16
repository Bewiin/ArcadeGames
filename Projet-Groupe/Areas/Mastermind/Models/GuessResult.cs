namespace Projet_Groupe.Areas.Mastermind.Models;

public class GuessResult
{
    public string[] Guess     { get; set; } = [];
    public int      BlackPins { get; set; }
    public int      WhitePins { get; set; }
}
