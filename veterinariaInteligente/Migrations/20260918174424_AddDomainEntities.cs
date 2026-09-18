using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace veterinariaInteligente.Migrations
{
    /// <inheritdoc />
    public partial class AddDomainEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MascotaEstado",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MascotaEstado", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Profesional",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombre = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    cedula = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    activo = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profesional", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Vacuna",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    activo = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vacuna", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Mascota",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    raza = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    sexo = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    fechaNacimiento = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    color = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    alergia = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    fechaArchivado = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    idDuenio = table.Column<int>(type: "INTEGER", nullable: false),
                    idEstado = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mascota", x => x.id);
                    table.ForeignKey(
                        name: "FK_Mascota_Duenio_idDuenio",
                        column: x => x.idDuenio,
                        principalTable: "Duenio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Mascota_MascotaEstado_idEstado",
                        column: x => x.idEstado,
                        principalTable: "MascotaEstado",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionCliente",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombre = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    telefono = table.Column<string>(type: "TEXT", maxLength: 15, nullable: false),
                    direccion = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    idProfesionalDefault = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionCliente", x => x.id);
                    table.ForeignKey(
                        name: "FK_ConfiguracionCliente_Profesional_idProfesionalDefault",
                        column: x => x.idProfesionalDefault,
                        principalTable: "Profesional",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "AplicacionVacuna",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    fechaAplicacion = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    lote = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    observacion = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    fechaProximaSugerida = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    idVacuna = table.Column<int>(type: "INTEGER", nullable: false),
                    idMascota = table.Column<int>(type: "INTEGER", nullable: false),
                    idProfesional = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AplicacionVacuna", x => x.id);
                    table.ForeignKey(
                        name: "FK_AplicacionVacuna_Mascota_idMascota",
                        column: x => x.idMascota,
                        principalTable: "Mascota",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AplicacionVacuna_Profesional_idProfesional",
                        column: x => x.idProfesional,
                        principalTable: "Profesional",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_AplicacionVacuna_Vacuna_idVacuna",
                        column: x => x.idVacuna,
                        principalTable: "Vacuna",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cita",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    fechaPrevista = table.Column<DateTime>(type: "TEXT", nullable: false),
                    motivo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    notas = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    estado = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    idMascota = table.Column<int>(type: "INTEGER", nullable: true),
                    idDuenio = table.Column<int>(type: "INTEGER", nullable: true),
                    idProfesional = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cita", x => x.id);
                    table.ForeignKey(
                        name: "FK_Cita_Duenio_idDuenio",
                        column: x => x.idDuenio,
                        principalTable: "Duenio",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Cita_Mascota_idMascota",
                        column: x => x.idMascota,
                        principalTable: "Mascota",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Cita_Profesional_idProfesional",
                        column: x => x.idProfesional,
                        principalTable: "Profesional",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "Consulta",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    motivo = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    peso = table.Column<decimal>(type: "decimal(5,3)", nullable: true),
                    observacion = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    tratamiento = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    diagnostico = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    estado = table.Column<string>(type: "TEXT", maxLength: 25, nullable: false),
                    idMascota = table.Column<int>(type: "INTEGER", nullable: false),
                    idProfesional = table.Column<int>(type: "INTEGER", nullable: false),
                    fechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    fechaModificacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consulta", x => x.id);
                    table.ForeignKey(
                        name: "FK_Consulta_Mascota_idMascota",
                        column: x => x.idMascota,
                        principalTable: "Mascota",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Consulta_Profesional_idProfesional",
                        column: x => x.idProfesional,
                        principalTable: "Profesional",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsultaVersion",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    fecha = table.Column<DateTime>(type: "datetime", nullable: false),
                    campo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    valorAnterior = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    valorNuevo = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    idConsulta = table.Column<int>(type: "INTEGER", nullable: false),
                    idProfesional = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultaVersion", x => x.id);
                    table.ForeignKey(
                        name: "FK_ConsultaVersion_Consulta_idConsulta",
                        column: x => x.idConsulta,
                        principalTable: "Consulta",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsultaVersion_Profesional_idProfesional",
                        column: x => x.idProfesional,
                        principalTable: "Profesional",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Seguimiento",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    motivo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    fechaPrevista = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    fechaRealizada = table.Column<DateOnly>(type: "TEXT", nullable: true),
                    notas = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    estado = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    idMascota = table.Column<int>(type: "INTEGER", nullable: false),
                    idConsultaOrigen = table.Column<int>(type: "INTEGER", nullable: true),
                    idAplicacionVacunaOrigen = table.Column<int>(type: "INTEGER", nullable: true),
                    idConsultaRealizacion = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seguimiento", x => x.id);
                    table.ForeignKey(
                        name: "FK_Seguimiento_AplicacionVacuna_idAplicacionVacunaOrigen",
                        column: x => x.idAplicacionVacunaOrigen,
                        principalTable: "AplicacionVacuna",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Seguimiento_Consulta_idConsultaOrigen",
                        column: x => x.idConsultaOrigen,
                        principalTable: "Consulta",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Seguimiento_Consulta_idConsultaRealizacion",
                        column: x => x.idConsultaRealizacion,
                        principalTable: "Consulta",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_Seguimiento_Mascota_idMascota",
                        column: x => x.idMascota,
                        principalTable: "Mascota",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AplicacionVacuna_idMascota",
                table: "AplicacionVacuna",
                column: "idMascota");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacionVacuna_idProfesional",
                table: "AplicacionVacuna",
                column: "idProfesional");

            migrationBuilder.CreateIndex(
                name: "IX_AplicacionVacuna_idVacuna",
                table: "AplicacionVacuna",
                column: "idVacuna");

            migrationBuilder.CreateIndex(
                name: "IX_Cita_idDuenio",
                table: "Cita",
                column: "idDuenio");

            migrationBuilder.CreateIndex(
                name: "IX_Cita_idMascota",
                table: "Cita",
                column: "idMascota");

            migrationBuilder.CreateIndex(
                name: "IX_Cita_idProfesional",
                table: "Cita",
                column: "idProfesional");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionCliente_idProfesionalDefault",
                table: "ConfiguracionCliente",
                column: "idProfesionalDefault");

            migrationBuilder.CreateIndex(
                name: "IX_Consulta_idMascota",
                table: "Consulta",
                column: "idMascota");

            migrationBuilder.CreateIndex(
                name: "IX_Consulta_idProfesional",
                table: "Consulta",
                column: "idProfesional");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultaVersion_idConsulta",
                table: "ConsultaVersion",
                column: "idConsulta");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultaVersion_idProfesional",
                table: "ConsultaVersion",
                column: "idProfesional");

            migrationBuilder.CreateIndex(
                name: "IX_Mascota_idDuenio",
                table: "Mascota",
                column: "idDuenio");

            migrationBuilder.CreateIndex(
                name: "IX_Mascota_idEstado",
                table: "Mascota",
                column: "idEstado");

            migrationBuilder.CreateIndex(
                name: "IX_Profesional_nombre",
                table: "Profesional",
                column: "nombre");

            migrationBuilder.CreateIndex(
                name: "IX_Seguimiento_idAplicacionVacunaOrigen",
                table: "Seguimiento",
                column: "idAplicacionVacunaOrigen");

            migrationBuilder.CreateIndex(
                name: "IX_Seguimiento_idConsultaOrigen",
                table: "Seguimiento",
                column: "idConsultaOrigen");

            migrationBuilder.CreateIndex(
                name: "IX_Seguimiento_idConsultaRealizacion",
                table: "Seguimiento",
                column: "idConsultaRealizacion");

            migrationBuilder.CreateIndex(
                name: "IX_Seguimiento_idMascota",
                table: "Seguimiento",
                column: "idMascota");

            migrationBuilder.CreateIndex(
                name: "IX_Vacuna_nombre",
                table: "Vacuna",
                column: "nombre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cita");

            migrationBuilder.DropTable(
                name: "ConfiguracionCliente");

            migrationBuilder.DropTable(
                name: "ConsultaVersion");

            migrationBuilder.DropTable(
                name: "Seguimiento");

            migrationBuilder.DropTable(
                name: "AplicacionVacuna");

            migrationBuilder.DropTable(
                name: "Consulta");

            migrationBuilder.DropTable(
                name: "Vacuna");

            migrationBuilder.DropTable(
                name: "Mascota");

            migrationBuilder.DropTable(
                name: "Profesional");

            migrationBuilder.DropTable(
                name: "MascotaEstado");
        }
    }
}
