using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    /// This sealed class is the same as the BooleanToVisibilityConverter
    /// but negated.
    /// </summary>
    [Localizability(LocalizationCategory.NeverLocalize)]
    public sealed class NegatedBoolToVisibilityConverter : IValueConverter {
        // Methods
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var flag = false;
            if (value is bool) {
                flag = (bool)value;
            }
            else if (value is bool?) {
                var nullable = (bool?)value;
                flag = nullable.HasValue ? nullable.Value : false;
            }
            return (!flag ? Visibility.Visible : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return ((value is Visibility) && (((Visibility)value) != Visibility.Visible));
        }
    }
}
