using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Gearset.Components.InspectorWPF {
    /// <summary>
    ///     This sealed class is the same as the BooleanToVisibilityConverter
    ///     but negated.
    /// </summary>
    [Localizability(LocalizationCategory.NeverLocalize)]
    public sealed class NullToVisibilityConverter : IValueConverter {
        // Methods
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException("NullToVisibilityConverter");
        }
    }
}
