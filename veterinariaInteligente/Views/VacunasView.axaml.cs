using Avalonia.Controls;
using veterinariaInteligente.Views.DesignTime;

namespace veterinariaInteligente.Views;

public partial class VacunasView : UserControl
{
    public VacunasView()
    {
        InitializeComponent();

#if DEBUG
        if (DataContext is null)
            DataContext = DesignTimeData.Vacunas;
#endif
    }
}