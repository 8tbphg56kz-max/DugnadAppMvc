using DugnadAppMvc.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DugnadAppMvc.ViewModels;

public class AdministrerPameldingerViewModel
{
    public Oppgave Oppgave { get; set; } = null!;

    public List<SelectListItem> LedigeBeboere { get; set; } = [];

    public int? ValgtBeboerId { get; set; }
}