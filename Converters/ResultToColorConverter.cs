using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using chession.Models;

namespace chession.Converters;

/// <summary>
/// Converts GameResult to a color for UI display.
/// </summary>
public class ResultToColorConverter : IValueConverter
{
    /// <summary>
    /// Gets the singleton instance of the converter.
    /// </summary>
    public static readonly ResultToColorConverter Instance = new();

    /// <summary>
    /// Converts a GameResult to a SolidColorBrush.
    /// </summary>
    /// <param name="value">The GameResult value.</param>
    /// <param name="targetType">The target type (ignored).</param>
    /// <param name="parameter">The converter parameter (ignored).</param>
    /// <param name="culture">The culture (ignored).</param>
    /// <returns>A SolidColorBrush representing the result color.</returns>
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

    /// <summary>
    /// Converts back is not supported.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The target type.</param>
    /// <param name="parameter">The converter parameter.</param>
    /// <param name="culture">The culture.</param>
    /// <returns>Throws NotImplementedException.</returns>
    /// <exception cref="NotImplementedException">Always thrown.</exception>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
