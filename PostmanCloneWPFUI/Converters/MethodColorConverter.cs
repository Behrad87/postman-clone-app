using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PostmanCloneWPFUI.Converters;

public class MethodColorConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        return (value as string)?.ToUpperInvariant() switch
        {
            "GET" => new SolidColorBrush(Color.FromRgb(0x61, 0xAF, 0xEF)),
            "POST" => new SolidColorBrush(Color.FromRgb(0x98, 0xC3, 0x79)),
            "PUT" => new SolidColorBrush(Color.FromRgb(0xE5, 0xC0, 0x7B)),
            "PATCH" => new SolidColorBrush(Color.FromRgb(0xC6, 0x78, 0xDD)),
            "DELETE" => new SolidColorBrush(Color.FromRgb(0xE0, 0x6C, 0x75)),
            "HEAD" => new SolidColorBrush(Color.FromRgb(0x56, 0xB6, 0xC2)),
            "OPTIONS" => new SolidColorBrush(Color.FromRgb(0xD1, 0x9A, 0x66)),
            _ => new SolidColorBrush(Colors.Gray)
        };
    }
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}
