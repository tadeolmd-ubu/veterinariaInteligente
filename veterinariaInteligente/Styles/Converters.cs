using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace veterinariaInteligente.Styles;

public class InicialesConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var nombre = value as string;
        if (string.IsNullOrWhiteSpace(nombre))
            return "?";

        var partes = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var iniciales = char.ToUpperInvariant(partes[0][0]).ToString();
        if (partes.Length > 1)
            iniciales += char.ToUpperInvariant(partes[1][0]);

        return iniciales;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}