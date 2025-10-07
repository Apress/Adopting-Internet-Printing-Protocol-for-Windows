using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BasicPsa.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TestPage : Page, INotifyPropertyChanged
    {
        public TestPage()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        bool printer;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool Printer
        {
            get { return printer; }
            set
            {
                printer = value;
                Debug.WriteLine($"Printer changed to: {value}");
                OnNotifyPropertyChanged();
            }
        }

        private void Button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Printer = false;

        }

        private void OnNotifyPropertyChanged([CallerMemberName] string sPropertyName = "")
        {
            if (PropertyChanged != null)

                PropertyChanged(this, new PropertyChangedEventArgs(sPropertyName));

        }

    }
}
