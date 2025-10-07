using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Printers;
using Windows.Graphics.Printing.PrintTicket;
using Windows.Graphics.Printing.PrintSupport;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static System.Collections.Specialized.BitVector32;
using Windows.Graphics.Printing3D;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Diagnostics;
using Windows.Graphics.Printing;
using System.Security.Cryptography;
using Windows.UI.Core;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BasicPsa
{
    /// <summary>
    /// Class for showing print settings to the user and allow user to modify it
    /// </summary>
    public sealed partial class DefaultSettingView : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private IppPrintDevice printer;
        private PrintSupportSettingsUISession uiSession;
        private WorkflowPrintTicket printTicket;
        private App application;
        List<PrintTicketOption> lst_options = new List<PrintTicketOption>();

        public PrintTicketOption SelectedOrientationOption { get; set; }
        private ObservableCollection<PrintTicketOption> m_orientationfeatureoptions = new ObservableCollection<PrintTicketOption>();
        public PrintTicketOption SelectedPageResolutionOption { get; set; }
        private ObservableCollection<PrintTicketOption> m_pageresolutionfeatureoptions = new ObservableCollection<PrintTicketOption>();

        public PrintTicketOption SelectedDocumentDuplexingOption { get; set; }
        private ObservableCollection<PrintTicketOption> m_documentduplexfeatureoptions = new ObservableCollection<PrintTicketOption>();

        public PrintTicketOption SelectedDocumentCollateOption { get; set; }
        private ObservableCollection<PrintTicketOption> m_documentcollatefeatureoptions = new ObservableCollection<PrintTicketOption>();

        public DefaultSettingView()
        {
            InitializeComponent();
            this.DataContext = this;
            this.application = (App)Application.Current;
        }

        public ObservableCollection<PrintTicketOption> OrientationFeatureOptions 
        { 
            get => m_orientationfeatureoptions;
            set
            {
                m_orientationfeatureoptions = value;
                OnNotifyPropertyChanged();
            }
        }

        public ObservableCollection<PrintTicketOption> PageResolutionFeatureOptions 
        { 
            get => m_pageresolutionfeatureoptions;
            set
            {
                m_pageresolutionfeatureoptions = value;
                OnNotifyPropertyChanged();
            }
        }

        public ObservableCollection<PrintTicketOption> DocumentDuplexFeatureOptions 
        { 
            get => m_documentduplexfeatureoptions;
            set
            {
                m_documentduplexfeatureoptions = value;
                OnNotifyPropertyChanged();
            }
        }

        public ObservableCollection<PrintTicketOption> DocumentCollateFeatureOptions 
        { 
            get => m_documentcollatefeatureoptions;
            set
            {
                m_documentcollatefeatureoptions = value;
                OnNotifyPropertyChanged();
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.uiSession = e.Parameter as PrintSupportSettingsUISession;
            this.printer = uiSession.SessionInfo.Printer;
            this.printTicket = uiSession.SessionPrintTicket;
            LoadFeatures();
        }

        private void LoadFeatures()
        {
            PrintTicketCapabilities printTicketCapabilities = this.printTicket.GetCapabilities();

            // Read orientation feature from PrintTicket capabilities
            PrintTicketFeature poFeature = printTicketCapabilities.PageOrientationFeature;
            // Populate XAML combo box with orientation feature options
            PopulateComboBox(OrientationFeatureOptions, poFeature.Options);

            //Look at current print ticket, get the current option that is selected
            PrintTicketOption printTicketOrientationOption = printTicket.PageOrientationFeature.GetSelectedOption();
            // Find a single option from the OrientationFeatureOptions collection that matches both the Name and the XmlNamespace of the 
            // PrintTicketOption. 
            SelectedOrientationOption = this.OrientationFeatureOptions.Single((option) => (option.Name == printTicketOrientationOption.Name && option.XmlNamespace == printTicketOrientationOption.XmlNamespace));
           
            //-- Page Resolution
            PrintTicketFeature prFeature = printTicketCapabilities.PageResolutionFeature;
            PopulateComboBox(PageResolutionFeatureOptions, prFeature.Options);
            PrintTicketOption printTicketPageResolutionOption = printTicket.PageResolutionFeature.GetSelectedOption();
           SelectedPageResolutionOption = this.PageResolutionFeatureOptions.Single((option) => (option.Name == printTicketPageResolutionOption.Name && option.XmlNamespace == printTicketPageResolutionOption.XmlNamespace));

            //-- Duplexing
            PrintTicketFeature pddFeature = printTicketCapabilities.DocumentDuplexFeature;
            PopulateComboBox(DocumentDuplexFeatureOptions, pddFeature.Options);
            PrintTicketOption printTicketDuplexingOption = printTicket.DocumentDuplexFeature.GetSelectedOption();
            SelectedDocumentDuplexingOption = this.DocumentDuplexFeatureOptions.Single((option) => (option.Name == printTicketDuplexingOption.Name && option.XmlNamespace == printTicketDuplexingOption.XmlNamespace));

            //-- Collating
            PrintTicketFeature dcFeature = printTicketCapabilities.DocumentCollateFeature;
            PopulateComboBox(DocumentCollateFeatureOptions, dcFeature.Options);
            PrintTicketOption printTicketCollateOption = printTicket.DocumentCollateFeature.GetSelectedOption();
            SelectedDocumentCollateOption = this.DocumentCollateFeatureOptions.Single((option) => (option.Name == printTicketCollateOption.Name && option.XmlNamespace == printTicketCollateOption.XmlNamespace));

        }

        private void PopulateComboBox(ObservableCollection<PrintTicketOption> col,IReadOnlyList<PrintTicketOption> _options)
        {
            foreach (PrintTicketOption pto in _options)
            {
                col.Add(pto);
            }
        }

        private async void OkClicked(object sender, RoutedEventArgs e)
        {
           
            // Disable Ok button while the print ticket is being submitted
            this.Ok.IsEnabled = false;

            // Set selected orientation option
            PrintTicketFeature orientationFeature = this.printTicket.PageOrientationFeature;
            orientationFeature.SetSelectedOption(this.SelectedOrientationOption);
            // Set selected resolution option
            PrintTicketFeature resolutionFeature = this.printTicket.PageResolutionFeature;
            resolutionFeature.SetSelectedOption(this.SelectedPageResolutionOption);
            // Set selected duplex option
            PrintTicketFeature duplexFeature = this.printTicket.DocumentDuplexFeature;
            duplexFeature.SetSelectedOption(this.SelectedDocumentDuplexingOption);
            // Set selected collate option
            PrintTicketFeature collateFeature = this.printTicket.DocumentCollateFeature;
            collateFeature.SetSelectedOption(this.SelectedDocumentCollateOption);

            // Validate and submit PrintTicket
            WorkflowPrintTicketValidationResult result = await printTicket.ValidateAsync();
            if (result.Validated)
            {
                // PrintTicket validated successfully – submit and exit
                this.uiSession.UpdatePrintTicket(printTicket);
                this.application.Exit();
            }
            else
            {
                // PrintTicket is not valid – show error
                CommonDialogs.ShowMessage(Defines.APP_NAME, result.ExtendedError.ToString());
            }

            this.Ok.IsEnabled = true;
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            this.application.Exit();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {

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
