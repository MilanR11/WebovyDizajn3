using System.ComponentModel.DataAnnotations;

namespace MakerslabInventory.Models
{
    public class Kategoria
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Názov kategórie je povinný")]
        public string Nazov { get; set; }
    }
}