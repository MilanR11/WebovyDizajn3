using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MakerslabInventory.Migrations
{
    /// <inheritdoc />
    public partial class FinalStavAsEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Nastav staré textové hodnoty (napr. 'Vypožičaný', 'ok', '9') na '0'.
            // To je nutné, aby sa predišlo chybe konverzie textu na číslo.
            migrationBuilder.Sql("UPDATE Inventar SET Stav = '0' WHERE Stav IS NOT NULL AND Stav NOT IN ('0', '1', '2', '3', '4', '5', '6', '7', '8', '9')");

            // 2. Bezpečne zmeň typ stĺpca 'Stav' z nvarchar(string) na int (Enum).
            migrationBuilder.AlterColumn<int>(
                name: "Stav",
                table: "Inventar",
                type: "int",
                nullable: false,
                defaultValue: 0, // Nastaví defaultnú hodnotu 0 (Dostupný) pre všetky riadky
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down metóda musí vrátiť stĺpec späť na string
            migrationBuilder.AlterColumn<string>(
                name: "Stav",
                table: "Inventar",
                type: "nvarchar(max)",
                nullable: false, // Predpokladáme, že pôvodne bol NOT NULL
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}

