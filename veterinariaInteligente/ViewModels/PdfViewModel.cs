using CommunityToolkit.Mvvm.ComponentModel;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.ViewModels;

public abstract partial class PdfViewModel : OperacionViewModel
{
    internal PdfService? ServicioPdf { get; private set; }
    public void ConfigurarPdf(PdfService servicio) => ServicioPdf = servicio;

    // Front observa PdfGenerado y guarda los bytes usando NombreArchivoPdf.
    [ObservableProperty] private byte[]? pdfGenerado;
    [ObservableProperty] private string? nombreArchivoPdf;

    protected void LimpiarPdf()
    {
        PdfGenerado = null;
        NombreArchivoPdf = null;
    }

    protected async Task GenerarPdfAsync(int? id, string tipo,
        Func<PdfService, int, Task<Resultado<byte[]>>> generar)
    {
        LimpiarPdf();
        if (ServicioPdf is null)
        {
            InformarPdf("El servicio de PDF no está configurado.");
            return;
        }
        if (id is null)
        {
            InformarPdf("Seleccioná el registro para generar el PDF.");
            return;
        }
        var resultado = await generar(ServicioPdf, id.Value);
        InformarPdf(resultado.Mensaje, resultado.Correcta);
        if (!resultado.Correcta || resultado.Valor is null) return;
        NombreArchivoPdf = $"{tipo}_{id}.pdf";
        // Publicar bytes al final para que Front tenga disponible el nombre al guardarlos.
        PdfGenerado = resultado.Valor;
    }

    protected abstract void InformarPdf(string mensaje, bool correcta = false);
}
