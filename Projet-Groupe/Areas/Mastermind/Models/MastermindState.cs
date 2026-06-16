namespace Projet_Groupe.Areas.Mastermind.Models;

public class MastermindState
{
    public string[]          Secret      { get; set; } = [];
    public List<GuessResult> History     { get; set; } = [];
    public GameStatus        Status      { get; set; } = GameStatus.InProgress;
    public int AttemptsLeft => MastermindConstants.MaxAttempts - History.Count;
}
