using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace InstallVibe.Converters;

public class CountToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; } = false;

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int count)
        {
            bool isVisible = count > 0;
            if (Invert)
            {
                isVisible = !isVisible;
            }
            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }
        return Invert ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
