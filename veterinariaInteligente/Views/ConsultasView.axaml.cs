using Avalonia.Controls;
using System.ComponentModel;
using veterinariaInteligente.ViewModels;
using veterinariaInteligente.Views.DesignTime;

namespace veterinariaInteligente.Views;

public partial class ConsultasView : UserControl
{
    private ConsultasViewModel? _viewModel;
    private PropertyChangedEventHandler? _pdfHandler;

    public ConsultasView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => ConectarViewModel(DataContext as ConsultasViewModel);

        if (Design.IsDesignMode)
            DataContext = DesignTimeData.Consultas;
    }

    private void ConectarViewModel(ConsultasViewModel? viewModel)
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
