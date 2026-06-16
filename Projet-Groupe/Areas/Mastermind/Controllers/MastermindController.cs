using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Projet_Groupe.Areas.Mastermind.Models;
using Projet_Groupe.Areas.Mastermind.Services;

namespace Projet_Groupe.Areas.Mastermind.Controllers;

[Area("Mastermind")]
[Authorize]
public class MastermindController : Controller
{
    private const string SessionKey = "MastermindState";
    private readonly IMastermindService _service;

    public MastermindController(IMastermindService service) => _service = service;

    public IActionResult Index() => View(GetStateFromSession());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Start()
    {
        SaveStateToSession(new MastermindState { Secret = _service.GenerateSecret() });
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Guess(GuessViewModel vm)
    {
        var state = GetStateFromSession();
        if (state is null || state.Status != GameStatus.InProgress)
            return RedirectToAction(nameof(Index));

        var guess = vm.ToArray();
        if (!guess.All(c => MastermindConstants.AvailableColors.Contains(c)))
            return RedirectToAction(nameof(Index));

        var result = _service.Evaluate(state.Secret, guess);
        state.History.Add(result);
        state.Status = _service.ResolveStatus(state);
        SaveStateToSession(state);

        return RedirectToAction(nameof(Index));
    }

    private MastermindState? GetStateFromSession()
    {
        var json = HttpContext.Session.GetString(SessionKey);
        return json is null ? null : JsonSerializer.Deserialize<MastermindState>(json);
    }

    private void SaveStateToSession(MastermindState state) =>
        HttpContext.Session.SetString(SessionKey, JsonSerializer.Serialize(state));
}
