using System.ComponentModel.DataAnnotations;
using MakerslabInventory.Models;

namespace MakerslabInventory.Models
{
    
    public class Inventar
    {
        // Pôvodné polia
        public int Id { get; set; } // Unikátne číslo pre každú vec

        [Required(ErrorMessage = "Názov je povinný.")]
        public string Nazov { get; set; } // Názov veci (napr. 3D tlačiareň Ender 3)

        public string Kategoria { get; set; } // Kategória (Tlačiareň, Filament, Nástroj, Súčiastka)

        [Required]
        [Range(0, 99999)]
        public int Mnozstvo { get; set; } // Aktuálny počet kusov/hmotnosť

        // NOVÉ POLIA PRE DETAILNEJŠIU INVENTARIZÁCIU

        [Display(Name = "Merná jednotka")]
        public string Jednotka { get; set; } = "ks"; // Merná jednotka (ks, kg, m, balenie)

        [Display(Name = "Umiestnenie")]
        public string Lokalita { get; set; } = "Makerslab"; // Učebňa, Dielňa, Sklad

        [Display(Name = "Min. limit")]
        [Range(0, 9999)]
        public int MinMnozstvo { get; set; } = 0; // Minimálne množstvo, kedy je potrebné objednať

        [Display(Name = "Max. limit")]
        [Range(1, 99999)]
        public int MaxMnozstvo { get; set; } = 100; // Maximálne množstvo, ktoré chceme mať na sklade

        // Polia pre SLEDovanie ZAPOŽIČANIA (len informácia)
        public StavInventara Stav { get; set; }

        [Display(Name = "Zapožičané komu")]
        public string? ZapozicaneKomu { get; set; } // Meno alebo ID používateľa

        [Display(Name = "Dátum výpožičky")]
        [DataType(DataType.Date)]
        public DateTime? DatumVypozicky { get; set; } // Kedy bolo zapožičané
    }
}