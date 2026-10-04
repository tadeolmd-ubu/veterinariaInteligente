using Avalonia.Controls;
using veterinariaInteligente.Views.DesignTime;

namespace veterinariaInteligente.Views;

public partial class PendientesView : UserControl
{
    public PendientesView()
    {
        InitializeComponent();

#if DEBUG
        if (DataContext is null)
            DataContext = DesignTimeData.Pendientes;
#endif
    }
}