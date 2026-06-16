using Projet_Groupe.Areas.Mastermind.Models;

namespace Projet_Groupe.Areas.Mastermind.Services;

public class MastermindService : IMastermindService
{
    public string[] GenerateSecret() =>
        Enumerable.Range(0, MastermindConstants.CodeLength)
            .Select(_ => MastermindConstants.AvailableColors[
                Random.Shared.Next(MastermindConstants.AvailableColors.Length)])
            .ToArray();

    public GuessResult Evaluate(string[] secret, string[] guess)
    {
        var secretRemaining = new List<string>();
        var guessRemaining  = new List<string>();
        int blacks = 0;

        for (int i = 0; i < MastermindConstants.CodeLength; i++)
        {
            if (secret[i] == guess[i])
                blacks++;
            else
            {
                secretRemaining.Add(secret[i]);
                guessRemaining.Add(guess[i]);
            }
        }

        int whites = 0;
        foreach (var color in guessRemaining)
        {
            int idx = secretRemaining.IndexOf(color);
            if (idx < 0) continue;
            whites++;
            secretRemaining.RemoveAt(idx);
        }

        return new GuessResult { Guess = guess, BlackPins = blacks, WhitePins = whites };
    }

    public GameStatus ResolveStatus(MastermindState state)
    {
        var last = state.History.LastOrDefault();
        if (last?.BlackPins == MastermindConstants.CodeLength) return GameStatus.Won;
        if (state.History.Count >= MastermindConstants.MaxAttempts) return GameStatus.Lost;
        return GameStatus.InProgress;
    }
}
