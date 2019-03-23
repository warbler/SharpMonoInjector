using System;
using System.Globalization;
using System.Windows.Data;
using SharpMonoInjector.Gui.Models;

namespace SharpMonoInjector.Gui.Converters
{
    public class MonoProcessToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            MonoProcess mp = (MonoProcess)value;
            return $"[{mp.Id}] {mp.Name}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
