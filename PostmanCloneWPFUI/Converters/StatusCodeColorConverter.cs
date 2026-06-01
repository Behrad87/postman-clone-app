using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PostmanCloneWPFUI.Converters;

public class StatusCodeColorConverter : IValueConverter
{
    public object Convert(object value, Type t, object p, CultureInfo c)
    {
        if (value is not int code) return new SolidColorBrush(Colors.Gray);
        return code switch
        {
            >= 200 and < 300 => new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)),
            >= 300 and < 400 => new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)),
            >= 400 and < 500 => new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)),
            >= 500 => new SolidColorBrush(Color.FromRgb(0x9C, 0x27, 0xB0)),
            _ => new SolidColorBrush(Color.FromRgb(0x9E, 0x9E, 0x9E))
        };
    }
    public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
}
