using FastEnumUtility;
using Kchary.PhotoViewer.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Kchary.PhotoViewer.Converter
{
    public sealed class BoolToSelectPageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter is not string parameterString)
            {
                return DependencyProperty.UnsetValue;
            }

            if (value is not SelectPage selectPageValue)
            {
                return DependencyProperty.UnsetValue;
            }

            if (!FastEnum.IsDefined<SelectPage>(parameterString))
            {
                return DependencyProperty.UnsetValue;
            }

            var parameterValue = FastEnum.Parse<SelectPage>(parameterString);
            return parameterValue == selectPageValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return parameter is not string parameterString ? DependencyProperty.UnsetValue : FastEnum.Parse<SelectPage>(parameterString);
        }
    }
}