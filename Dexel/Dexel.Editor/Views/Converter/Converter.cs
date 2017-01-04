using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Dexel.Editor.ViewModels.DrawingBoard;

namespace Dexel.Editor.Views.Converter
{

    public class IntegrationBorderTopPointConverter : IMultiValueConverter
    {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var pos = (Point) values[0];
            var width = (double)values[1];
            var height = (double)values[2];
            return new Point(pos.X +width / 2, pos.Y + height);
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PercentageConverter : MarkupExtension, IValueConverter
    {
        private static PercentageConverter _instance;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new PercentageConverter());
        }
    }


    public class TextDocumentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var document = new ICSharpCode.AvalonEdit.Document.TextDocument {Text = (string) value};

            return document;
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DebugConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
           return value;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MissingTypesButtonText : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int) value == 0 ? "All Types Defined" : $"Create Missing Types ({value})";
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FunctionUnitTextfieldBG : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility =  (Visibility)value;
            return visibility == Visibility.Visible ? Brushes.Transparent 
                : Application.Current.Resources["FillFunctionUnitBrush"];
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selected = (bool)value;
            if (selected)
            {

                return Visibility.Visible;
            }

            return Visibility.Hidden;

        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ValidationFlagVisbilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flag = (ValidationFlag)value;
            return flag == ValidationFlag.Valid ? Visibility.Hidden : Visibility.Visible;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ValidationFlagColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flag = (ValidationFlag)value;
            if (flag == ValidationFlag.Invalid)
                return Application.Current.Resources["ErrorColor"];
            if (flag == ValidationFlag.Warning)
            {
                return Application.Current.Resources["WarningColor"];
            }
            return null;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public class ValidationFlagFillConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flag = (ValidationFlag)value;
            if (flag == ValidationFlag.Invalid)
                return Application.Current.Resources["ErrorColor20"];
            if (flag == ValidationFlag.Warning)
            {
                return Application.Current.Resources["WarningColor20"];
            }
            return null;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }



    public class FunctionUnitSelectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selected = (bool)value;
            if (selected)
            {

                return new SolidColorBrush(Colors.DodgerBlue);
            }

            return new  SolidColorBrush(Colors.Transparent);

        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}



