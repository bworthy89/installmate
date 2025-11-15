using Microsoft.UI.Xaml.Data;
using System;

namespace InstallVibe.Converters;

public class BoolToCompleteTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is bool isCompleted)
        {
            return isCompleted ? "Mark Incomplete" : "Mark Complete";
        }
        return "Mark Complete";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
