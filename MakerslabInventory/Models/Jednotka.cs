using System.ComponentModel.DataAnnotations;

namespace MakerslabInventory.Models
{
    public class Jednotka
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Názov jednotky je povinný")]
        public string Nazov { get; set; }
    }
}