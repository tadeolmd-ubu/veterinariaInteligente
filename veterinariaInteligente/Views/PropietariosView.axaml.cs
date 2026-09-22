using Avalonia.Controls;
using veterinariaInteligente.Views.DesignTime;

namespace veterinariaInteligente.Views;

public partial class PropietariosView : UserControl
{
    public PropietariosView()
    {
        InitializeComponent();

#if DEBUG
        if (DataContext is null)
            DataContext = DesignTimeData.Propietarios;
#endif
    }
}