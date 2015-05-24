using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace NodeMachine.ViewModel.Converter
{
    public class ValueConverterChain
        : List<IValueConverter>, IValueConverter
    {
        //http://stackoverflow.com/a/8326207/108234

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return this.Aggregate(value, (current, converter) => converter.Convert(current, targetType, parameter, culture));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((IEnumerable<IValueConverter>)this).Reverse().Aggregate(value, (current, converter) => converter.ConvertBack(current, targetType, parameter, culture));
        }
    }
}
