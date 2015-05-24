using System;
using System.Windows.Data;

namespace NodeMachine.ViewModel.Converter
{
    [ValueConversion(typeof(string), typeof(bool))]
    public class IsStringNullConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
