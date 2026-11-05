using Avalonia.Controls;
using System.ComponentModel;
using veterinariaInteligente.ViewModels;
using veterinariaInteligente.Views.DesignTime;

namespace veterinariaInteligente.Views;

public partial class VacunasView : UserControl
{
    private VacunasViewModel? _viewModel;
    private PropertyChangedEventHandler? _pdfHandler;

    public VacunasView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => ConectarViewModel(DataContext as VacunasViewModel);

        if (Design.IsDesignMode)
            DataContext = DesignTimeData.Vacunas;
    }

    private void ConectarViewModel(VacunasViewModel? viewModel)
    {
        if (_viewModel is not null && _pdfHandler is not null)
            _viewModel.PropertyChanged -= _pdfHandler;
        _viewModel = viewModel;
        _pdfHandler = null;
        if (_viewModel is null) return;

        _pdfHandler = (_, e) =>
        {
            if (e.PropertyName == nameof(PdfViewModel.PdfGenerado))
                _ = StorageDialogHelper.GuardarPdfGeneradoAsync(this, _viewModel);
        };
        _viewModel.PropertyChanged += _pdfHandler;
    }
}
