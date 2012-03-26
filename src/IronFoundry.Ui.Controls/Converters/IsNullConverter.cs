namespace IronFoundry.Ui.Controls.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    
    public class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }

            if (!(value is bool))
            {
                return true;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
