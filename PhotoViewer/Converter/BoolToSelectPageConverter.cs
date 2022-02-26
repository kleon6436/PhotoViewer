using System;
using System.Globalization;
using System.Windows.Data;
using FastEnumUtility;
using Kchary.PhotoViewer.ViewModels;

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

            if (value is not SettingViewModel.SelectPage selectPageValue)
            {
                return System.Windows.DependencyProperty.UnsetValue;
            }

            if (FastEnum.IsDefined<SettingViewModel.SelectPage>(parameterString) == false)
            {
                return System.Windows.DependencyProperty.UnsetValue;
            }

            var parameterValue = FastEnum.Parse<SettingViewModel.SelectPage>(parameterString);
            return parameterValue == selectPageValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter is not string parameterString ? System.Windows.DependencyProperty.UnsetValue : FastEnum.Parse<SettingViewModel.SelectPage>(parameterString);
        }
    }
}