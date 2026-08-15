using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace PostmanCloneWPFUI.Converters
{

    public class ReferenceEqualityConverter : IMultiValueConverter
    {
        public object Convert(object[] values,
            Type targetType,
            object parameter,
            CultureInfo culture)
        {
            if (values.Length < 2)
                return false;

            return ReferenceEquals(values[0], values[1]);
        }

        public object[] ConvertBack(
            object value,
            Type[] targetTypes,
            object parameter,
            CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
