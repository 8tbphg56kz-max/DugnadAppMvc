using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.ViewModels
{
    public class EndrePassordViewModel
    {
        [Required(ErrorMessage = "Skriv inn ditt nåværende passord.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nåværende passord")]
        public string GammeltPassord { get; set; } = "";

        [Required(ErrorMessage = "Skriv inn et nytt passord.")]
        [StringLength(100, ErrorMessage = "Passordet må være minst {2} tegn langt.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Nytt passord")]
        public string NyttPassord { get; set; } = "";

        [DataType(DataType.Password)]
        [Compare("NyttPassord", ErrorMessage = "Passordene er ikke like.")]
        [Display(Name = "Bekreft nytt passord")]
        public string BekreftPassord { get; set; } = "";
    }
}