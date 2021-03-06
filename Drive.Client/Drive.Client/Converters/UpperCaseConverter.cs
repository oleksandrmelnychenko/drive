﻿using System;
using System.Globalization;
using Xamarin.Forms;

namespace Drive.Client.Converters {
    public class UpperCaseConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null) return null;
            string result = value as string;
            return result.ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return value?.ToString().ToUpper();
        }
    }
}
