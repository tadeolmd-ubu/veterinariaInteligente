using Avalonia.Controls;
using Avalonia.Platform.Storage;
using veterinariaInteligente.ViewModels;

namespace veterinariaInteligente.Views;

internal static class StorageDialogHelper
{
    public static async Task<string?> SeleccionarCarpetaAsync(Control control)
    {
        var topLevel = TopLevel.GetTopLevel(control);
        if (topLevel is null) return null;

        var carpetas = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Selecciona la carpeta de respaldo",
            AllowMultiple = false
        });

        return carpetas.FirstOrDefault()?.Path.LocalPath;
    }

    public static async Task GuardarPdfAsync(Control control, byte[] contenido, string nombreArchivo)
    {
        var topLevel = TopLevel.GetTopLevel(control);
        if (topLevel is null) return;

        var archivo = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Guardar documento PDF",
            SuggestedFileName = nombreArchivo,
            DefaultExtension = "pdf",
            FileTypeChoices =
            [
                new FilePickerFileType("Documento PDF")
                {
                    Patterns = ["*.pdf"],
                    MimeTypes = ["application/pdf"]
                }
            ]
        });

        if (archivo is null) return;

        await using var flujo = await archivo.OpenWriteAsync();
        await flujo.WriteAsync(contenido);
    }

    public static async Task GuardarPdfGeneradoAsync(Control control, PdfViewModel viewModel)
    {
        if (viewModel.PdfGenerado is not { Length: > 0 } contenido ||
            string.IsNullOrWhiteSpace(viewModel.NombreArchivoPdf)) return;

        try
        {
            await GuardarPdfAsync(control, contenido, viewModel.NombreArchivoPdf);
        }
        catch (Exception ex)
        {
            if (TopLevel.GetTopLevel(control) is Window ventana)
                await new MensajeWindow(new MensajeDialogo("No se pudo guardar el PDF", ex.Message,
                    TipoMensajeOperacion.Error)).ShowDialog(ventana);
        }
    }
}
