using Microsoft.UI.Xaml.Data;
using System;

namespace InstallVibe.Converters;

public class EmptyStringToBoolConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object value, Type targetType, object parameter, string language)
    {
        bool isEmpty = string.IsNullOrEmpty(value as string);

        if (Invert)
            return isEmpty;

        return !isEmpty; // Not empty = true
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
