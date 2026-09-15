using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace veterinariaInteligente.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Duenio",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    telefono = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    correo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    direccion = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    fechaArchivado = table.Column<DateOnly>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Duenio", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Duenio_nombre",
                table: "Duenio",
                column: "nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Duenio_telefono",
                table: "Duenio",
                column: "telefono");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Duenio");
        }
    }
}
