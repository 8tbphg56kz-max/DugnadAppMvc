using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.Models.Enums
{
    public enum OppgavePrioritet
    {
        [Display(Name = "Lav")]
        Lav = 1,

        [Display(Name = "Normal")]
        Normal = 2,

        [Display(Name = "Høy")]
        Høy = 3
    }
}