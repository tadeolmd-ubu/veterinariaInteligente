using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace veterinariaInteligente.Migrations
{
    /// <inheritdoc />
    public partial class SeedMascotaEstado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MascotaEstado",
                columns: new[] { "id", "nombre" },
                values: new object[,]
                {
                    { 1, "Activa" },
                    { 2, "Inactiva" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MascotaEstado",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MascotaEstado",
                keyColumn: "id",
                keyValue: 2);
        }
    }
}
