using System.Globalization;

namespace app_subastas.Converters;

public class EstadoColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var estado = value?.ToString() ?? "";
        return estado switch
        {
            "ACTIVA" => Colors.LimeGreen,    // Verde para subastas activas
            "PENDIENTE" => Colors.Orange,   // Naranja para pendientes
            "FINALIZADA" => Colors.Red,     // Rojo para finalizadas
            _ => Colors.Gray                // Gris por defecto
        };
    }

    // ConvertBack no se utiliza en este converter
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}