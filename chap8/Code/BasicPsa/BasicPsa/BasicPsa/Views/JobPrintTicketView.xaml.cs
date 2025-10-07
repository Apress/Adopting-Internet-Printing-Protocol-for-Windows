using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
using System.Threading.Tasks;
using Windows.Graphics.Printing;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.Graphics.Printing.PrintSupport;
using Windows.Graphics.Printing.PrintTicket;
using Windows.Devices.Printers;
using System.Xml.Linq;
using System.Collections.ObjectModel;
using Windows.UI.Popups;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BasicPsa
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class JobPrintTicketView : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private IppPrintDevice printer;
        private PrintSupportSettingsUISession uiSession;
        private App application;
        public IDictionary<string, IppAttributeValue> vals= new Dictionary<string, IppAttributeValue>();
        bool bIsProcessed = false;
        bool m_bEdgeToEdge;
        bool m_bEnhancedMixedMode;
        bool m_bTabbedPrinting;

        public bool EdgeToEdge 
        { 
            get => m_bEdgeToEdge;
            set
            {
                m_bEdgeToEdge = value;
                OnNotifyPropertyChanged();
            }
        }
        public bool EnhancedMixedMode 
        { 
            get => m_bEnhancedMixedMode;
            set
            {
                m_bEnhancedMixedMode = value;
                OnNotifyPropertyChanged();
            }
        }
        public bool TabbedPrinting 
        { 
            get => m_bTabbedPrinting;
            set
            {
                m_bTabbedPrinting = value;
                OnNotifyPropertyChanged();
            }
        }

        public JobPrintTicketView()
        {
            this.InitializeComponent();
            this.DataContext = this;
            this.application = (App)Application.Current;
        }

        
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            this.uiSession = e.Parameter as PrintSupportSettingsUISession;
            this.printer = uiSession.SessionInfo.Printer; 
            await GetSpecialAttributes();
        }
        

        /// <summary>
        /// GetSpecialAttributes
        /// 
        /// Do a get-printer-attributes query on the printer to get special attributes:
        /// 
        /// hp-edge-to-edge-support
        /// hp-enhanced-mixed-orientation-supported
        /// hp-tabbed-printing-supported
        /// 
        /// These are not actual special attributes as per HP engineers, but serve as an
        /// example in how to get them.
        /// </summary>
        /// <returns></returns>
        private async Task GetSpecialAttributes()
        {
            try
            {
                IDictionary<string, IppAttributeValue> attributes = await Ipp.GetVendorSpecialAttributesAsync(printer.PrinterName);

                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    foreach (var attribute in attributes)
                    {
                        if (attribute.Key == "hp-edge-to-edge-support")
                        {
                            EdgeToEdge = (bool)attribute.Value.GetBooleanArray().FirstOrDefault();
                        }
                        else if (attribute.Key == "hp-enhanced-mixed-orientation-supported")
                        {
                            EnhancedMixedMode = (bool)attribute.Value.GetBooleanArray().FirstOrDefault();
                        }
                        else if (attribute.Key == "hp-tabbed-printing-supported")
                        {
                            TabbedPrinting = (bool)attribute.Value.GetBooleanArray().FirstOrDefault();
                        }
                    }
                });
            }
            catch(Exception ex)
            {
                MessageDialog dialog = new MessageDialog("Hello, the issue is" + ex.Message);
                await dialog.ShowAsync();
            }
            
        }
       


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
           
        }

        private async void OkClicked(object sender, RoutedEventArgs e)
        {
            //If these were real attributes, we would send the attribute changes to the printer here...
            this.application.Exit();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            this.application.Exit();
        }

        /// <summary>
        /// OnNotifyPropertyChanged
        /// </summary>
        /// <param name="sPropertyName"></param>
        private void OnNotifyPropertyChanged([CallerMemberName] string sPropertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(sPropertyName));
        }

   
    }
}
