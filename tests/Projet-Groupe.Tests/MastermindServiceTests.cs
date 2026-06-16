using Projet_Groupe.Areas.Mastermind.Models;
using Projet_Groupe.Areas.Mastermind.Services;

namespace Projet_Groupe.Tests;

public class MastermindServiceTests
{
    private readonly MastermindService _sut = new();

    [Fact]
    public void Evaluate_CodeExact_Retourne4Noirs0Blancs()
    {
        string[] secret = ["Rouge", "Jaune", "Vert", "Bleu"];
        string[] guess  = ["Rouge", "Jaune", "Vert", "Bleu"];

        var result = _sut.Evaluate(secret, guess);

        Assert.Equal(4, result.BlackPins);
        Assert.Equal(0, result.WhitePins);
    }

    [Fact]
    public void Evaluate_AucuneCorrespondance_Retourne0Noirs0Blancs()
    {
        string[] secret = ["Rouge", "Rouge", "Rouge", "Rouge"];
        string[] guess  = ["Bleu",  "Bleu",  "Bleu",  "Bleu"];

        var result = _sut.Evaluate(secret, guess);

        Assert.Equal(0, result.BlackPins);
        Assert.Equal(0, result.WhitePins);
    }

    [Fact]
    public void Evaluate_CouleursBienMaisPositionsFausses_Retourne0Noirs4Blancs()
    {
        string[] secret = ["Rouge", "Jaune", "Vert",  "Bleu"];
        string[] guess  = ["Bleu",  "Vert",  "Jaune", "Rouge"];

        var result = _sut.Evaluate(secret, guess);

        Assert.Equal(0, result.BlackPins);
        Assert.Equal(4, result.WhitePins);
    }

    [Fact]
    public void Evaluate_CasMixte_RetourneBonsComptes()
    {
        // Rouge bien placé (noir), Jaune+Vert mal placés (blancs), Violet absent
        string[] secret = ["Rouge", "Jaune", "Vert",   "Bleu"];
        string[] guess  = ["Rouge", "Vert",  "Jaune",  "Violet"];

        var result = _sut.Evaluate(secret, guess);

        Assert.Equal(1, result.BlackPins);
        Assert.Equal(2, result.WhitePins);
    }

    [Fact]
    public void Evaluate_DoublonsDansSecret_ComptageCorrect()
    {
        // Secret: Rouge Rouge Jaune Vert — Guess: Rouge Bleu Rouge Orange
        // Noirs: position 0 (Rouge=Rouge) = 1
        // Restant secret [Rouge,Jaune,Vert], restant guess [Bleu,Rouge,Orange]
        // → Rouge trouvé 1 fois dans secret restant = 1 blanc
        string[] secret = ["Rouge", "Rouge", "Jaune",  "Vert"];
        string[] guess  = ["Rouge", "Bleu",  "Rouge",  "Orange"];

        var result = _sut.Evaluate(secret, guess);

        Assert.Equal(1, result.BlackPins);
        Assert.Equal(1, result.WhitePins);
    }

    [Fact]
    public void ResolveStatus_4Noirs_RetourneWon()
    {
        var state = new MastermindState
        {
            Secret  = ["Rouge", "Jaune", "Vert", "Bleu"],
            History = [new GuessResult { Guess = ["Rouge", "Jaune", "Vert", "Bleu"], BlackPins = 4, WhitePins = 0 }],
            Status  = GameStatus.InProgress,
        };

        var status = _sut.ResolveStatus(state);

        Assert.Equal(GameStatus.Won, status);
    }

    [Fact]
    public void ResolveStatus_HistoriquePlein_RetourneLost()
    {
        var history = Enumerable.Range(0, MastermindConstants.MaxAttempts)
            .Select(_ => new GuessResult { Guess = ["Bleu", "Bleu", "Bleu", "Bleu"], BlackPins = 0, WhitePins = 0 })
            .ToList();

        var state = new MastermindState
        {
            Secret  = ["Rouge", "Jaune", "Vert", "Orange"],
            History = history,
            Status  = GameStatus.InProgress,
        };

        var status = _sut.ResolveStatus(state);

        Assert.Equal(GameStatus.Lost, status);
    }

    [Fact]
    public void GenerateSecret_RetourneTableauDe4CouleursValides()
    {
        var secret = _sut.GenerateSecret();

        Assert.Equal(MastermindConstants.CodeLength, secret.Length);
        Assert.All(secret, c => Assert.Contains(c, MastermindConstants.AvailableColors));
    }

    [Fact]
    public void ResolveStatus_9Essais_RetourneInProgress()
    {
        var history = Enumerable.Range(0, MastermindConstants.MaxAttempts - 1)
            .Select(_ => new GuessResult { Guess = ["Bleu", "Bleu", "Bleu", "Bleu"], BlackPins = 0, WhitePins = 0 })
            .ToList();

        var state = new MastermindState
        {
            Secret  = ["Rouge", "Jaune", "Vert", "Orange"],
            History = history,
            Status  = GameStatus.InProgress,
        };

        var status = _sut.ResolveStatus(state);

        Assert.Equal(GameStatus.InProgress, status);
    }

    [Fact]
    public void GenerateSecret_PlusieursAppels_PeutProduireDesDoublons()
    {
        // Vérifie que le secret autorise les répétitions (comportement voulu du jeu)
        bool hasDoublon = false;
        for (int i = 0; i < 200 && !hasDoublon; i++)
        {
            var secret = _sut.GenerateSecret();
            hasDoublon = secret.Length != secret.Distinct().Count();
        }
        Assert.True(hasDoublon, "Après 200 appels, au moins un secret avec doublon devrait apparaître.");
    }

    [Fact]
    public void Evaluate_GuessIdentique_RetourneGuessPreserve()
    {
        string[] secret = ["Rouge", "Jaune", "Vert", "Bleu"];
        string[] guess  = ["Rouge", "Jaune", "Vert", "Bleu"];

        var result = _sut.Evaluate(secret, guess);

        Assert.Equal(guess, result.Guess);
    }
}
