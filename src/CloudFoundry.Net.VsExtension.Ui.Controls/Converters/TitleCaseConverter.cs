namespace CloudFoundry.Net.VsExtension.Ui.Controls.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

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