using System;
using System.Globalization;
using System.Windows.Data;

namespace NodeMachine.ViewModel.Converter
{
    [ValueConversion(typeof(long), typeof(string))]
    public class LongToFileSizeConverter
        : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] units = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            double size = (long)value;
            int unit = 0;

            while (size >= 1000)
            {
                size /= 1024;
                ++unit;
            }

            return String.Format("{0:0.#}{1}", size, units[unit]);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
