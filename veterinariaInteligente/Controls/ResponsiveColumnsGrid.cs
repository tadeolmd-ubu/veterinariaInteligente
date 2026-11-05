using Avalonia;
using Avalonia.Controls;

namespace veterinariaInteligente.Controls;

/// <summary>
/// Grid que reparte sus hijos en varias columnas y reduce la cantidad de
/// columnas cuando la ventana se achica. Sirve para tarjetas y bloques de datos.
/// </summary>
public sealed class ResponsiveColumnsGrid : Grid
{
    private int _currentColumns;
    private int _currentChildren;

    /// <summary>Cantidad de columnas en ventanas grandes.</summary>
    public int WideColumns { get; set; } = 4;

    /// <summary>Cantidad de columnas en ventanas chicas.</summary>
    public int NarrowColumns { get; set; } = 2;

    /// <summary>Ancho a partir del cual se usa la configuración angosta.</summary>
    public double Breakpoint { get; set; } = 780;

    /// <summary>Separación horizontal y vertical entre hijos.</summary>
    public double Spacing { get; set; } = 16;

    /// <summary>Separación vertical opcional; si es 0 se usa <see cref="Spacing"/>.</summary>
    public double VerticalSpacing { get; set; }

    public ResponsiveColumnsGrid()
    {
        ColumnSpacing = Spacing;
        RowSpacing = VerticalSpacing > 0 ? VerticalSpacing : Spacing;
        SizeChanged += (_, e) => UpdateColumns(e.NewSize.Width);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        UpdateColumns(availableSize.Width);
        return base.MeasureOverride(availableSize);
    }

    private void UpdateColumns(double width)
    {
        if (Children.Count == 0)
            return;

        var narrow = double.IsFinite(width) && width > 0 && width < Breakpoint;
        var columns = Math.Max(narrow ? NarrowColumns : WideColumns, 1);

        if (columns == _currentColumns && Children.Count == _currentChildren)
            return;

        _currentColumns = columns;
        _currentChildren = Children.Count;

        ColumnDefinitions = new ColumnDefinitions(string.Join(",", Enumerable.Repeat("*", columns)));

        var rows = Math.Max((int)Math.Ceiling(Children.Count / (double)columns), 1);
        RowDefinitions = new RowDefinitions(string.Join(",", Enumerable.Repeat("Auto", rows)));
        ColumnSpacing = Spacing;
        RowSpacing = VerticalSpacing > 0 ? VerticalSpacing : Spacing;

        for (var i = 0; i < Children.Count; i++)
        {
            SetColumn(Children[i], i % columns);
            SetRow(Children[i], i / columns);
            SetColumnSpan(Children[i], 1);
            SetRowSpan(Children[i], 1);
        }
    }
}
