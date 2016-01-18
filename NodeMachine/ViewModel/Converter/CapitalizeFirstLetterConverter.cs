using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace NodeMachine.ViewModel.Converter
{
    [ValueConversion(typeof(string), typeof(string))]
    public class CapitalizeFirstLetterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = (string)value;
            if (string.IsNullOrWhiteSpace(str))
                return value;

            return string.Join(" ", str.Split(' ').Select(w => char.ToUpper(w[0]) + w.Substring(1)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string)value).ToLowerInvariant();
        }
    }
}