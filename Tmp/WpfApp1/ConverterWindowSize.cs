using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace CwaConverter
{
    [ValueConversion(typeof(double), typeof(double))]
    public class ConverterWindowSize : IValueConverter
    {
        // Get int from int
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double r = Double.Parse(parameter as string, CultureInfo.InvariantCulture);
            double result = (double)value * r;
#if DEBUG
            Debug.WriteLine($"Converted: {value} to {result}");
#endif
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
