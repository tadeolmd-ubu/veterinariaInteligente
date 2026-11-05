using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using veterinariaInteligente.Data;
using veterinariaInteligente.Models;
using veterinariaInteligente.Models.Enums;

namespace veterinariaInteligente.Service;

public class PdfService
{
    static PdfService()
    {
        // QuestPDF exige declarar la licencia antes de generar el primer documento.
        // Community: gratis para particulares, proyectos académicos/open source
        // y organizaciones con ingresos anuales menores a 1 millón de USD.
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private readonly AppDbContext _db;

    public PdfService(AppDbContext db) => _db = db;

    public async Task<Resultado<byte[]>> GenerarConsultaAsync(
        int idConsulta)
    {
        var consulta = await _db.Consultas
            .AsNoTracking()
            .Include(c => c.Mascota)
            .Include(c => c.Profesional)
            .FirstOrDefaultAsync(c => c.Id == idConsulta);

        if (consulta is null)
        {
            return new Resultado<byte[]>(
                false,
                "La consulta no existe.",
                null);
        }

        var configuracion = await ObtenerConfiguracionAsync();

        return GenerarDocumento(
            "Consulta veterinaria",
            configuracion,
            consulta.Profesional?.Nombre ?? configuracion?.ProfesionalDefault?.Nombre,
            columna =>
            {
                AgregarDatosMascota(columna, consulta.Mascota);
                AgregarConsulta(columna, consulta);
            });
    }

    public async Task<Resultado<byte[]>> GenerarExpedienteAsync(
        int idMascota)
    {
        var mascota = await _db.Mascotas
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == idMascota);

        if (mascota is null)
        {
            return new Resultado<byte[]>(
                false,
                "La mascota no existe.",
                null);
        }

        var consultas = await _db.Consultas
            .AsNoTracking()
            .Include(c => c.Profesional)
            .Where(c =>
                c.IdMascota == idMascota &&
                c.Estado != EstadoConsulta.Archivada)
            .OrderByDescending(c => c.FechaCreacion)
            .ThenByDescending(c => c.Id)
            .ToListAsync();

        var configuracion = await ObtenerConfiguracionAsync();
        var veterinario = consultas.FirstOrDefault()?.Profesional?.Nombre
                          ?? configuracion?.ProfesionalDefault?.Nombre;

        return GenerarDocumento(
            "Historial de consultas",
            configuracion,
            veterinario,
            columna =>
            {
                AgregarDatosMascota(columna, mascota);

                columna.Item()
                    .PaddingTop(10)
                    .Text("Consultas")
                    .FontSize(14)
                    .SemiBold();

                if (consultas.Count == 0)
                {
                    columna.Item()
                        .Text("No hay consultas registradas para mostrar.");

                    return;
                }

                foreach (var consulta in consultas)
                    AgregarConsulta(columna, consulta);
            });
    }

    public async Task<Resultado<byte[]>> GenerarCartillaVacunacionAsync(
        int idMascota)
    {
        var mascota = await _db.Mascotas
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == idMascota);

        if (mascota is null)
        {
            return new Resultado<byte[]>(
                false,
                "La mascota no existe.",
                null);
        }

        var aplicaciones = await _db.AplicacionVacunas
            .AsNoTracking()
            .Include(a => a.Vacuna)
            .Include(a => a.Profesional)
            .Where(a => a.IdMascota == idMascota)
            .OrderByDescending(a => a.FechaAplicacion)
            .ThenByDescending(a => a.Id)
            .ToListAsync();

        var configuracion = await ObtenerConfiguracionAsync();
        var veterinario = aplicaciones.FirstOrDefault()?.Profesional?.Nombre
                          ?? configuracion?.ProfesionalDefault?.Nombre;

        return GenerarDocumento(
            "Cartilla de vacunación",
            configuracion,
            veterinario,
            columna =>
            {
                AgregarDatosMascota(columna, mascota);

                columna.Item()
                    .PaddingTop(10)
                    .Text("Vacunas aplicadas")
                    .FontSize(14)
                    .SemiBold();

                if (aplicaciones.Count == 0)
                {
                    columna.Item()
                        .Text("No hay aplicaciones de vacunas registradas.");

                    return;
                }

                foreach (var aplicacion in aplicaciones)
                    AgregarAplicacion(columna, aplicacion);
            });
    }

    private static Resultado<byte[]> GenerarDocumento(
        string titulo,
        ConfiguracionCliente? configuracion,
        string? veterinario,
        Action<ColumnDescriptor> contenido)
    {
        var nombreVeterinaria = string.IsNullOrWhiteSpace(configuracion?.Nombre)
            ? "Veterinaria"
            : configuracion.Nombre.Trim();
        var contacto = string.Join(" · ", new[]
                { configuracion?.Telefono, configuracion?.Direccion }
            .Where(valor => !string.IsNullOrWhiteSpace(valor))
            .Select(valor => valor!.Trim()));
        var nombreVeterinario = string.IsNullOrWhiteSpace(veterinario)
            ? "Profesional responsable"
            : veterinario.Trim();

        var documento = Document.Create(contenedor =>
        {
            contenedor.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.MarginHorizontal(42);
                pagina.MarginVertical(38);

                pagina.DefaultTextStyle(estilo =>
                    estilo.FontSize(10).FontColor("#263746"));

                pagina.Header()
                    .PaddingBottom(24)
                    .Background("#0A2E4F")
                    .PaddingHorizontal(22)
                    .PaddingVertical(17)
                    .Row(fila =>
                    {
                        fila.RelativeItem().Column(columna =>
                        {
                            columna.Item()
                                .Text(titulo)
                                .FontSize(19)
                                .SemiBold()
                                .FontColor(Colors.White);
                            columna.Item()
                                .PaddingTop(4)
                                .Text("DOCUMENTO CLÍNICO VETERINARIO")
                                .FontSize(8)
                                .SemiBold()
                                .FontColor("#91DCEB");
                        });

                        fila.ConstantItem(220).AlignRight().Column(columna =>
                        {
                            columna.Item()
                                .AlignRight()
                                .Text(nombreVeterinaria)
                                .FontSize(14)
                                .SemiBold()
                                .FontColor(Colors.White);

                            if (contacto.Length > 0)
                                columna.Item()
                                    .PaddingTop(4)
                                    .AlignRight()
                                    .Text(contacto)
                                    .FontSize(8)
                                    .FontColor("#D2E2EC");
                        });
                    });

                pagina.Content().Column(columna =>
                {
                    columna.Spacing(7);
                    contenido(columna);

                    columna.Item()
                        .ExtendVertical()
                        .AlignBottom()
                        .PaddingTop(38)
                        .Row(fila =>
                        {
                            fila.RelativeItem();
                            fila.ConstantItem(290).Column(firma =>
                            {
                                firma.Item()
                                    .LineHorizontal(1)
                                    .LineColor("#0A2E4F");
                                firma.Item()
                                    .PaddingTop(7)
                                    .AlignCenter()
                                    .Text(texto =>
                                    {
                                        texto.Span("Firma del veterinario  ")
                                            .SemiBold()
                                            .FontColor("#0A2E4F");
                                        texto.Span(nombreVeterinario)
                                            .FontColor("#526777");
                                    });
                            });
                        });
                });

                pagina.Footer()
                    .PaddingTop(12)
                    .BorderTop(1)
                    .BorderColor("#DCE6ED")
                    .PaddingTop(8)
                    .Row(fila =>
                    {
                        fila.RelativeItem()
                            .Text(nombreVeterinaria)
                            .FontSize(8)
                            .FontColor("#7A8D9B");
                        fila.RelativeItem()
                            .AlignRight()
                            .Text(texto =>
                            {
                                texto.Span("Página ").FontSize(8).FontColor("#7A8D9B");
                                texto.CurrentPageNumber().FontSize(8).FontColor("#7A8D9B");
                                texto.Span(" de ").FontSize(8).FontColor("#7A8D9B");
                                texto.TotalPages().FontSize(8).FontColor("#7A8D9B");
                            });
                    });
            });
        });

        var bytes = documento.GeneratePdf();

        return new Resultado<byte[]>(
            true,
            "PDF generado.",
            bytes);
    }

    private Task<ConfiguracionCliente?> ObtenerConfiguracionAsync() =>
        _db.ConfiguracionesCliente
            .AsNoTracking()
            .Include(configuracion => configuracion.ProfesionalDefault)
            .OrderBy(configuracion => configuracion.Id)
            .FirstOrDefaultAsync();

    private static void AgregarDatosMascota(
        ColumnDescriptor columna,
        Mascota mascota)
    {
        columna.Item()
            .Text($"Mascota: {mascota.Nombre}")
            .FontSize(14)
            .SemiBold();

        AgregarCampo(columna, "Raza", mascota.Raza);
        AgregarCampo(columna, "Sexo", mascota.Sexo);
        AgregarCampo(columna, "Color", mascota.Color);

        AgregarCampo(
            columna,
            "Fecha de nacimiento",
            mascota.FechaNacimiento?.ToString("dd/MM/yyyy"));

        AgregarCampo(columna, "Alergias", mascota.Alergia);
    }

    private static void AgregarConsulta(
        ColumnDescriptor columna,
        Consulta consulta)
    {
        AgregarSeparador(columna);

        columna.Item()
            .Text(
                $"Consulta #{consulta.Id} — " +
                $"{consulta.FechaCreacion:dd/MM/yyyy HH:mm}")
            .SemiBold();

        AgregarCampo(
            columna,
            "Profesional",
            consulta.Profesional?.Nombre);

        AgregarCampo(columna, "Estado", consulta.Estado.ToString());
        AgregarCampo(columna, "Motivo", consulta.Motivo);

        AgregarCampo(
            columna,
            "Peso",
            consulta.Peso?.ToString("0.##"));

        AgregarCampo(columna, "Diagnóstico", consulta.Diagnostico);
        AgregarCampo(columna, "Tratamiento", consulta.Tratamiento);
        AgregarCampo(columna, "Observación", consulta.Observacion);
    }

    private static void AgregarAplicacion(
        ColumnDescriptor columna,
        AplicacionVacuna aplicacion)
    {
        AgregarSeparador(columna);

        columna.Item()
            .Text(aplicacion.Vacuna.Nombre)
            .SemiBold();

        AgregarCampo(
            columna,
            "Fecha de aplicación",
            aplicacion.FechaAplicacion.ToString("dd/MM/yyyy"));

        AgregarCampo(columna, "Lote", aplicacion.Lote);

        AgregarCampo(
            columna,
            "Profesional",
            aplicacion.Profesional?.Nombre);

        AgregarCampo(
            columna,
            "Próxima fecha sugerida",
            aplicacion.FechaProximaSugerida?.ToString("dd/MM/yyyy"));

        AgregarCampo(
            columna,
            "Observación",
            aplicacion.Observacion);
    }

    private static void AgregarCampo(
        ColumnDescriptor columna,
        string etiqueta,
        string? valor)
    {
        var texto = string.IsNullOrWhiteSpace(valor)
            ? "Sin registrar"
            : valor.Trim();

        columna.Item().Text(contenido =>
        {
            contenido.Span($"{etiqueta}: ").SemiBold();
            contenido.Span(texto);
        });
    }

    private static void AgregarSeparador(
        ColumnDescriptor columna)
    {
        columna.Item()
            .PaddingVertical(8)
            .LineHorizontal(1)
            .LineColor(Colors.Grey.Lighten2);
    }
}
