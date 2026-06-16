using System.ComponentModel.DataAnnotations;

namespace Projet_Groupe.Areas.Mastermind.Models;

public class GuessViewModel
{
    [Required] public string Color1 { get; set; } = "";
    [Required] public string Color2 { get; set; } = "";
    [Required] public string Color3 { get; set; } = "";
    [Required] public string Color4 { get; set; } = "";

    public string[] ToArray() => [Color1, Color2, Color3, Color4];
}
