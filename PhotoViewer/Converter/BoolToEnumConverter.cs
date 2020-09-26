using System;
using System.Globalization;
using System.Windows.Data;

namespace Kchary.PhotoViewer.Converter
{
    public sealed class BoolToEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(parameter is string ParameterString))
            {
                return System.Windows.DependencyProperty.UnsetValue;
            }

            if (Enum.IsDefined(value.GetType(), value) == false)
            {
                return System.Windows.DependencyProperty.UnsetValue;
            }

            var paramvalue = Enum.Parse(value.GetType(), ParameterString);

            return (int)paramvalue == (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(parameter is string ParameterString) ? System.Windows.DependencyProperty.UnsetValue : Enum.Parse(targetType, ParameterString);
        }
    }
}