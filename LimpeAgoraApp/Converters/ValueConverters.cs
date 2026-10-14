using System.Globalization;
using LimpeAgora.Shared.Enums;

namespace LimpeAgoraApp.Converters;

public class StringToBooleanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return !string.IsNullOrWhiteSpace(value as string);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class InvertedBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }
}

public class IntToBooleanConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is int i && i > 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StatusPendenteConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is StatusPedido s && s == StatusPedido.Pendente;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class StatusToColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is StatusPedido s)
        {
            return s switch
            {
                StatusPedido.Pendente => Color.FromArgb("#D97706"),           // Âmbar / Amarelo
                StatusPedido.EmPreparacao => Color.FromArgb("#0284C7"),       // Ciano Limpeza
                StatusPedido.ProntoParaRetirada => Color.FromArgb("#1E40AF"), // Azul Royal
                StatusPedido.Concluido => Color.FromArgb("#15803D"),          // Verde Sucesso
                StatusPedido.Cancelado => Color.FromArgb("#DC2626"),          // Vermelho
                _ => Color.FromArgb("#64748B")
            };
        }
        return Color.FromArgb("#64748B");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

public class StatusToBgColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is StatusPedido s)
        {
            return s switch
            {
                StatusPedido.Pendente => Color.FromArgb("#FEF3C7"),           // Amarelo Suave
                StatusPedido.EmPreparacao => Color.FromArgb("#E0F2FE"),       // Ciano Suave
                StatusPedido.ProntoParaRetirada => Color.FromArgb("#EFF6FF"), // Azul Suave
                StatusPedido.Concluido => Color.FromArgb("#F0FDF4"),          // Verde Suave
                StatusPedido.Cancelado => Color.FromArgb("#FEE2E2"),          // Vermelho Suave
                _ => Color.FromArgb("#F1F5F9")
            };
        }
        return Color.FromArgb("#F1F5F9");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}

