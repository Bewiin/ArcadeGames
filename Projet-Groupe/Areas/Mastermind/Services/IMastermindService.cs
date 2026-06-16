using Projet_Groupe.Areas.Mastermind.Models;

namespace Projet_Groupe.Areas.Mastermind.Services;

public interface IMastermindService
{
    string[]    GenerateSecret();
    GuessResult Evaluate(string[] secret, string[] guess);
    GameStatus  ResolveStatus(MastermindState state);
}
