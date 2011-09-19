using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls;
using System.Globalization;

namespace CloudFoundry.Net.VsExtension.Ui.Controls.Converters
{
    public class CsvConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] == null || values[1] == null) return null;
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue) return null;

            string strValue = values[0].ToString();
            ItemCollection ic = (ItemCollection)values[1];
            int intCollectionCount = ic.Count;

            int intCurrentIndex = ic.IndexOf(strValue);
            if (intCurrentIndex < intCollectionCount - 1)
                return strValue + ", ";
            else
                return strValue;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
