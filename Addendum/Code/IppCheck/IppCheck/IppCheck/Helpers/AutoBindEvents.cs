using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace IppCheck
{
    public class AutoBindEvents
    {
        public static bool GetIsAutoBindEvent(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsAutoBindEventProperty);
        }

        public static void SetIsAutoBindEvent(DependencyObject obj, bool value)
        {
            obj.SetValue(IsAutoBindEventProperty, value);
        }


        //Use a DependencyProperty as ThemeDictionaryExtension backing store for IsAutoBindEvent. This enables animation, styling, binding, etc.
        public static readonly DependencyProperty IsAutoBindEventProperty =
            DependencyProperty.RegisterAttached("IsAutoBindEvent", typeof(bool), typeof(AutoBindEvents), new PropertyMetadata(false, OnPropertyChanged));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var win = d as Window;
            if(win != null)
            {
                win.DataContextChanged += (sender, ev) =>
                {
                    //Get your view model
                    var vm = win.DataContext as IViewModel;
                    vm.MyWindow = win;
                };
                win.Loaded += (sender, ev) =>
                {
                    var vm = win.DataContext as IViewModel;
                    vm.OnLoaded(sender, ev);
                };
                win.Closed += (sender, ev) =>
                {
                    var vm = win.DataContext as IViewModel;
                    vm.OnClose(sender, ev);
                };
                win.Closing += (sender, ev) =>
                 {
                     var vm = win.DataContext as IViewModel;
                     vm.OnClosing(sender, ev);
                 };
            }
        }
    }

}
