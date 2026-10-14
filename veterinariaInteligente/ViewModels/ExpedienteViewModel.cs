using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using veterinariaInteligente.Models;
using veterinariaInteligente.Service;

namespace veterinariaInteligente.ViewModels;

public partial class ExpedienteViewModel : PdfViewModel
{
    private readonly ConsultaService? _consultaService;

    public ExpedienteViewModel()
    {
        MascotaActual = null!;
        DuenioActual = null!;
    }

    public ExpedienteViewModel(Mascota mascota, Duenio duenio)
        : this(mascota, duenio, null)
    {
    }

    public ExpedienteViewModel(
        Mascota mascota,
        Duenio duenio,
        ConsultaService? consultaService, PdfService? pdfService = null)
    {
        MascotaActual = mascota;
        DuenioActual = duenio;
        _consultaService = consultaService;
        if (pdfService is not null) ConfigurarPdf(pdfService);
    }

    public Mascota MascotaActual { get; }
    public Duenio DuenioActual { get; }
    public ObservableCollection<Consulta> ConsultasItems { get; } = [];

    [ObservableProperty]
    private Consulta? selectedConsulta;

    [ObservableProperty]
    private string mensajeOperacion = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    protected override void InformarPdf(string mensaje, bool correcta = false)
        => MensajeOperacion = Notificar(mensaje, correcta);

    [RelayCommand]
    private async Task GenerarExpedientePdfAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            await GenerarPdfAsync(MascotaActual?.Id, "expediente",
                (pdf, id) => pdf.GenerarExpedienteAsync(id));
        }
        catch (Exception ex) { MensajeOperacion = Notificar($"No se pudo generar el PDF: {ex.Message}"); }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task CargarAsync()
    {
        if (_consultaService is null || IsBusy)
            return;

        IsBusy = true;

        try
        {
            var resultado = await _consultaService.ObtenerExpedienteAsync(MascotaActual.Id);
            MensajeOperacion = Notificar(resultado.Mensaje, resultado.Correcta);
            ConsultasItems.Clear();
            SelectedConsulta = null;

            if (!resultado.Correcta || resultado.Valor is null)
                return;

            foreach (var consulta in resultado.Valor)
                ConsultasItems.Add(consulta);

            SelectedConsulta = ConsultasItems.FirstOrDefault();
        }
        catch (Exception ex)
        {
            ConsultasItems.Clear();
            SelectedConsulta = null;
            MensajeOperacion = Notificar($"No se pudo cargar el expediente: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
