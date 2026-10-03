using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using veterinariaInteligente.Models.Enums;

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

public class EstadoConsultaConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not EstadoConsulta estado)
            return null;

        var parametro = parameter?.ToString();

        if (parametro == "nombre")
            return estado switch
            {
                EstadoConsulta.Pendiente => "Pendiente",
                EstadoConsulta.EnCurso => "En curso",
                EstadoConsulta.Finalizada => "Finalizada",
                EstadoConsulta.Archivada => "Archivada",
                _ => estado.ToString()
            };

        var (fondo, texto) = estado switch
        {
            EstadoConsulta.Pendiente => ("#FFF4E0", "#A8701B"),
            EstadoConsulta.EnCurso => ("#EAF2FD", "#2F8FD1"),
            EstadoConsulta.Finalizada => ("#EAFCEF", "#2EBD85"),
            EstadoConsulta.Archivada => ("#EEF2F6", "#5C6F80"),
            _ => ("#EEF2F6", "#5C6F80")
        };

        return new SolidColorBrush(Color.Parse(parametro == "texto" ? texto : fondo));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class TabEstadoConverter : IValueConverter
{
    public bool EsFondo { get; set; }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var archivadas = value is true;
        var activa = parameter?.ToString() == "Archivadas" ? archivadas : !archivadas;

        var color = EsFondo
            ? (activa ? "#0A2E4F" : "#FFFFFF")
            : (activa ? "#FFFFFF" : "#142B3C");

        return new SolidColorBrush(Color.Parse(color));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class VistaActualConverter : IValueConverter
{
    public string Modo { get; set; } = "fondo";

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var activa = parameter?.ToString() is { } nombre && value?.GetType().Name == nombre;

        return Modo switch
        {
            "texto" => new SolidColorBrush(Color.Parse(activa ? "#F4FBFF" : "#0A2E4F")),
            "barra" => (double)(activa ? 1 : 0),
            _ => new SolidColorBrush(Color.Parse(activa ? "#0A2E4F" : "#5FFFFFFF"))
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}