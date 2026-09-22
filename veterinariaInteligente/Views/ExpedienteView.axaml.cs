using Avalonia.Controls;
using veterinariaInteligente.Views.DesignTime;

namespace veterinariaInteligente.Views;

public partial class ExpedienteView : UserControl
{
    public ExpedienteView()
    {
        InitializeComponent();

#if DEBUG
        if (DataContext is null)
            DataContext = DesignTimeData.Expediente;
#endif
    }
}