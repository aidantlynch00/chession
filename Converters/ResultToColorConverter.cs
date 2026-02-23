using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using chession.Models;

namespace chession.Converters;

public class ResultToColorConverter : IValueConverter
{
    public static readonly ResultToColorConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value switch
        {
            GameResult.Win => new SolidColorBrush(Color.Parse("#18A34A")),
            GameResult.Loss => new SolidColorBrush(Color.Parse("#DD2627")),
            GameResult.Draw => new SolidColorBrush(Color.Parse("#EBB30F")),
            null => new SolidColorBrush(Color.Parse("#3A3A3A")),
            _ => new SolidColorBrush(Color.Parse("#666666"))
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
