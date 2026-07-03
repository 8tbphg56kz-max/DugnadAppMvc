using System.ComponentModel.DataAnnotations;

namespace DugnadAppMvc.Models
{
    public class Sameie
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Navn { get; set; } = "";
    }
}