using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Microsoft.Xna.Framework;

namespace Gearset.Components {
    /// <summary>
    ///     This sealed class is the same as the BooleanToVisibilityConverter
    ///     but negated.
    /// </summary>
    [Localizability(LocalizationCategory.NeverLocalize)]
    public sealed class CurveLoopTypeToBooleanConverter : IValueConverter {
        // Methods
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value.ToString() == (string)parameter)
                return true;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Enum.Parse(typeof(CurveLoopType), (string)parameter);
        }
    }
}
