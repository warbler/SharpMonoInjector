using System;
using System.Globalization;
using System.Windows.Data;
using SharpMonoInjector.Gui.Models;

namespace SharpMonoInjector.Gui.Converters
{
    public class InjectedAssemblyToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            InjectedAssembly ia = (InjectedAssembly)value;
            return $"[{(ia.Is64Bit ? $"0x{ia.Address.ToInt64():X16}" : $"0x{ia.Address.ToInt32():X8}")}] {ia.Name}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
