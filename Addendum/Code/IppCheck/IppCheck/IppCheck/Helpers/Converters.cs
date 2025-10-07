using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;
using IppCheck;
using System.Windows.Media.Imaging;

namespace IppCheck
{
    public class ReverseBooleanToVisibilityConverter : IValueConverter
    {
        //ThreadPriority
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var b = (bool)value;
            if (b)
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return false;
        }
    }

    public class BoolToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool val = (bool)value;
            if (val)
            {
                return "pack://application:,,,/IppCheck;component/Graphics/pass.png";
            }
            else
            {
                return "pack://application:,,,/IppCheck;component/Graphics/fail.png";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToColorModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool val = (bool)value;
            if (val)
            {
                return "pack://application:,,,/IppCheck;component/Graphics/color.png";
            }
            else
            {
                return "pack://application:,,,/IppCheck;component/Graphics/bw.png";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToIppUsabilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool val = (bool)value;
            if (val)
            {
                return "pack://application:,,,/IppCheck;component/Graphics/GreenLight.png";
            }
            else
            {
                return "pack://application:,,,/IppCheck;component/Graphics/RedLight.png";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// IppReachabilityConverter
    /// 
    /// Converts AttributeItem.PrinterIppStatus to a graphic depicting the status
    /// </summary>
    [ValueConversion(typeof(IppPrinter.PrinterIppStatus), typeof(string))]
    public class IppReachabilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((IppPrinter.PrinterIppStatus)value)
            {
                case IppPrinter.PrinterIppStatus.UNREACHABLE:
                    return "pack://application:,,,/IppCheck;component/Graphics/unreachable.png";
                case IppPrinter.PrinterIppStatus.REACHABLE_CONFIGURED:
                    return "pack://application:,,,/IppCheck;component/Graphics/ippconfigured.png";
                case IppPrinter.PrinterIppStatus.REACHABLE_NOT_CONFIGURED:
                    return "pack://application:,,,/IppCheck;component/Graphics/ippnotconfigured.png";
                default:
                    return "pack://application:,,,/IppCheck;component/Graphics/unknown.png";

            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    

    
}
