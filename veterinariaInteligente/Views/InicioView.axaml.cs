using Avalonia.Controls;
using veterinariaInteligente.Views.DesignTime;

namespace veterinariaInteligente.Views;

public partial class InicioView : UserControl
{
    public InicioView()
    {
        InitializeComponent();

#if DEBUG
        if (DataContext is null)
            DataContext = DesignTimeData.Inicio;
#endif
    }
}