using MicroMvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.Runtime.CompilerServices;
using CsIppRequestLib;
using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Data.Common;

namespace IppCheck
{
    public class JobCreationWindowViewModel : INotifyPropertyChanged, IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string m_sPrinterName;
        private UserSettings us = UserSettings.GetInstance;
        private int m_iRequestNumber = 1;
        private IppAttributeCollection m_pac;
        private ObservableCollection<JobCreationItem> m_sJobCreationAttributes = new ObservableCollection<JobCreationItem>();
        private string m_sDefaultValue = string.Empty;  

        public JobCreationWindowViewModel()
        {
           
        }

        //Commands
        public ICommand ExitCommand { get { return new RelayCommand(ExecuteExitCommand, EvaluateExitCommand); } }
        public ICommand DefaultCommand { get { return new RelayCommand<JobCreationItem>(ExecuteGetDefault, EvaluateGetDefault); } }

        /// <summary>
        /// Pac 
        /// 
        /// Collection of printer attributes returned when using
        /// the 'Attributes' button to query printer
        /// </summary>
        public IppAttributeCollection Pac
        {
            get => m_pac;
            set
            {
                m_pac = value;
                OnNotifyPropertyChanged();
            }
        }

        public Window MyWindow { get; set; }

        /// <summary>
        /// JobCreationAttributes 
        /// </summary>
        public ObservableCollection<JobCreationItem> JobCreationAttributes 
        { 
            get => m_sJobCreationAttributes;
            set
            {
                m_sJobCreationAttributes = value;
                OnNotifyPropertyChanged();
            }
        }
        /// <summary>
        /// PrinterName
        /// 
        /// Name of the printer info was extracted from
        /// </summary>
        public string PrinterName
        {
            get => m_sPrinterName;
            set
            {
                m_sPrinterName = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// DefaultValue
        /// 
        /// Bound to textbox, is the default job creation attribute value   
        /// </summary>
        public string DefaultValue
        {
            get => m_sDefaultValue;
            set
            {
                m_sDefaultValue = value;
                OnNotifyPropertyChanged();
            }
        }

        public void OnLoaded(object sender, EventArgs e)
        {
            PrinterName = us.SelectedPrinter.QueryName;
            Pac = new IppAttributeCollection();
            ExecuteAttributesCommandAsync();
        }

        /// <summary>
        /// ExecuteAttributesCommandAsync
        /// 
        /// Performs the Get-Printer-Attributes request and places the
        /// results in a collection of IppAttribute objets, Pac. This 
        /// method awaits the return of the IPP request.
        /// </summary>
        private async void ExecuteAttributesCommandAsync()
        {
            //Clear the existing observable collection
            m_iRequestNumber++;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                GetIppAttributesRequest gpa = new GetIppAttributesRequest(us.IppVersion, m_sPrinterName, us.Ipps, m_iRequestNumber);
                CompletionStruct cs = await gpa.SendRequestAsync();
                if (IppHelpers.IsRequestSuccessful(cs.status))
                {
                    IEnumerator<IppAttribute> pas = gpa.GetAttributeValues();
                    while (pas.MoveNext())
                    {
                        IppAttribute pa = pas.Current as IppAttribute;
                        Pac.Add(pa);
                    }
                    IppAttribute pja = gpa.Pac.Find("job-creation-attributes-supported");
                    if (pja != null)
                    {
                        foreach(string ja in pja.AttributeValues)
                        {
                            JobCreationItem jca = new JobCreationItem(ja);
                            JobCreationAttributes.Add(jca);  
                        }
                        GetItemValues();
                    }
                }
                else
                {
                    MessageBox.Show(string.Format("Get-Printer-Attributes request failed, reason: {0}", cs.status), Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
                }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }



        /// <summary>
        /// GetItemValues
        /// 
        /// Get the default values for all the possible job creation attributes.
        /// </summary>
        private void GetItemValues()
        {
            foreach (JobCreationItem jci in JobCreationAttributes)
            {
                IppAttribute ipa = Pac.Find(jci.Name + "-supported");
                if (ipa != null)
                {
                    jci.Value = ipa.WriteAttributeValuesString();
                }

                IppAttribute ipd = Pac.Find(jci.Name + "-default");
                if (ipd != null)
                {
                    jci.Default = ipd.WriteAttributeValuesString();
                }
            }
        }

        private bool EvaluateGetDefault(JobCreationItem selectedItem)
        {
            return selectedItem != null;
        }

        private void ExecuteGetDefault(JobCreationItem selectedItem)
        {
            if (selectedItem != null)
            {
                // Use the selectedItem.Default property or other logic
                DefaultValue = selectedItem.Default;
            }
        }

        public void OnClose(object sender, EventArgs e)
        {
            MyWindow.Close();
        }

        public void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }


        private bool EvaluateExitCommand()
        {
            return true;
        }

        private void ExecuteExitCommand()
        {
            OnClose(this, null);
        }

        /// <summary>
        /// OnNotifyPropertyChanged
        /// </summary>
        /// <param name="sPropertyName"></param>
        private void OnNotifyPropertyChanged([CallerMemberName] string sPropertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(sPropertyName));
        }
    }
}
