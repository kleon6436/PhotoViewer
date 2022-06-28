using FastEnumUtility;
using Kchary.PhotoViewer.ViewModels;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Kchary.PhotoViewer.Converter
{
    public sealed class BoolToSelectPageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is not string parameterString)
            {
                return System.Windows.DependencyProperty.UnsetValue;
            }

            if (value is not SelectPage selectPageValue)
            {
                return System.Windows.DependencyProperty.UnsetValue;
            }

            if (!FastEnum.IsDefined<SelectPage>(parameterString))
            {
                return System.Windows.DependencyProperty.UnsetValue;
            }

            var parameterValue = FastEnum.Parse<SelectPage>(parameterString);
            return parameterValue == selectPageValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter is not string parameterString ? System.Windows.DependencyProperty.UnsetValue : FastEnum.Parse<SelectPage>(parameterString);
        }
    }
}