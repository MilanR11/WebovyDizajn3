namespace MakerslabInventory.Models
{
    public enum StavInventara
    {
        // Tieto hodnoty sa v databáze uložia ako čísla (0, 1, 2)
        Dostupný = 0,
        Vypožičaný = 1,
        Pokazený = 2,
        NizkaZasoba = 3
    }
}
