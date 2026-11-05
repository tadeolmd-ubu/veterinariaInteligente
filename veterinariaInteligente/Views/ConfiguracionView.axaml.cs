using Avalonia.Controls;
using veterinariaInteligente.ViewModels;
using veterinariaInteligente.Views.DesignTime;

namespace veterinariaInteligente.Views;

public partial class ConfiguracionView : UserControl
{
    public ConfiguracionView()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => ConectarViewModel(DataContext as ConfiguracionViewModel);

        if (Design.IsDesignMode)
            DataContext = DesignTimeData.Configuracion;
    }

    private ConfiguracionViewModel? _viewModel;

    private void ConectarViewModel(ConfiguracionViewModel? viewModel)
    {
        if (_viewModel is not null)
            _viewModel.SeleccionCarpetaRespaldoSolicitada -= SeleccionarCarpetaAsync;
        _viewModel = viewModel;
        if (_viewModel is not null)
            _viewModel.SeleccionCarpetaRespaldoSolicitada += SeleccionarCarpetaAsync;
    }

    private Task<string?> SeleccionarCarpetaAsync() => StorageDialogHelper.SeleccionarCarpetaAsync(this);
}
