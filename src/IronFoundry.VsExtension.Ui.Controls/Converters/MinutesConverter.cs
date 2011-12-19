namespace CloudFoundry.Net.VsExtension.Ui.Controls.Converters
{
    using System;
    using System.Windows.Data;

    public class MinutesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var minutes = string.Empty;
            if (value != null)
            {
                TimeSpan ts = TimeSpan.FromSeconds((int)value);
                if (ts.Hours != 0)
                    minutes = string.Format("{0}:{1:00}:{2:00} hours", ts.Hours, ts.Minutes, ts.Seconds);
                else if (ts.Minutes == 0)
                    minutes = string.Format("{0} seconds", ts.Seconds);                
                else
                    minutes = string.Format("{0}:{1:00} minutes", ts.Minutes, ts.Seconds);                                                    
            }
            return minutes;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}