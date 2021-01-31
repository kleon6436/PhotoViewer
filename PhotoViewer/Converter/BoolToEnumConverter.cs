using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace Kchary.PhotoViewer.Converter
{
    public sealed class BoolToEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not string parameterString)
            {
                return System.Windows.DependencyProperty.UnsetValue;
            }

            if (value != null && Enum.IsDefined(value.GetType(), value) == false)
            {
                return System.Windows.DependencyProperty.UnsetValue;
            }

            Debug.Assert(value != null, nameof(value) + " != null");
            var parameterValue = Enum.Parse(value.GetType(), parameterString);

            return (int)parameterValue == (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter is not string parameterString ? System.Windows.DependencyProperty.UnsetValue : Enum.Parse(targetType, parameterString);
        }
    }
}