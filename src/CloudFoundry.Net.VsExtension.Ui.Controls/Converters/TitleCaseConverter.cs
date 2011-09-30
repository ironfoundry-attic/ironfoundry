using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Converters
{
    public class TitleCaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            var stringValue = value as string;
            stringValue = stringValue.ToLower();
            stringValue = culture.TextInfo.ToTitleCase(stringValue);
            return stringValue;            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
