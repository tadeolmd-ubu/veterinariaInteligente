using Avalonia;
using Avalonia.Controls;

namespace veterinariaInteligente.Controls;

/// <summary>
/// Grid de dos columnas (listado + detalle) que reduce el ancho del listado
/// cuando la ventana se achica, para que el detalle conserve espacio útil.
/// </summary>
public sealed class ResponsiveTwoPane : Grid
{
    private double _lastListWidth = -1;

    /// <summary>Ancho máximo del panel de listado en ventanas grandes.</summary>
    public double ExpandedListWidth { get; set; } = 390;

    /// <summary>Ancho mínimo del panel de listado en ventanas chicas.</summary>
    public double MinListWidth { get; set; } = 240;

    /// <summary>Proporción del ancho disponible que ocupa el listado.</summary>
    public double ListWidthRatio { get; set; } = 0.42;

    /// <summary>Separación entre las dos columnas.</summary>
    public double Spacing { get; set; } = 22;

    public ResponsiveTwoPane()
    {
        ColumnDefinitions = new ColumnDefinitions($"{ExpandedListWidth:0.##},*");
        ColumnSpacing = Spacing;
        SizeChanged += (_, e) => UpdateColumns(e.NewSize.Width);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        UpdateColumns(availableSize.Width);
        return base.MeasureOverride(availableSize);
    }

    private void UpdateColumns(double width)
    {
        if (Children.Count < 2)
            return;

        var listWidth = double.IsFinite(width) && width > 0
            ? Math.Clamp(width * ListWidthRatio, MinListWidth, ExpandedListWidth)
            : ExpandedListWidth;

        if (Math.Abs(listWidth - _lastListWidth) < 0.5)
            return;

        _lastListWidth = listWidth;
        ColumnDefinitions = new ColumnDefinitions(
            FormattableString.Invariant($"{listWidth:0.##},*"));
        ColumnSpacing = Spacing;

        for (var i = 0; i < Children.Count; i++)
        {
            SetColumn(Children[i], i == 0 ? 0 : 1);
            SetRow(Children[i], 0);
        }
    }
}
