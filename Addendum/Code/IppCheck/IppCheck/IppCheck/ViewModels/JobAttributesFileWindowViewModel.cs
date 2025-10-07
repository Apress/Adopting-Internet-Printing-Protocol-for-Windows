using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MicroMvvm;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace IppCheck
{
    public class JobAttributesFileWindowViewModel : INotifyPropertyChanged, IViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private JobAttributesListEditor _jale;
        private ObservableCollection<string> lstJobAttributes = new ObservableCollection<string>();
        private string m_sSelectedJobAttribute;
        private string m_sNewJobAttribute;
        private IppAttributeCollection m_pac;


        public JobAttributesFileWindowViewModel()
        {
            _jale = JobAttributesListEditor.GetInstance;
        }
        public ICommand ExitCommand { get { return new RelayCommand(ExecuteExitCommand, EvaluateExitCommand); } }
        public ICommand AddJobAttributeCommand { get { return new RelayCommand(ExecuteAddJobAttributeCommand, EvaluateAddJobAttributeCommand); } }
        public ICommand RemoveAttributeCommand { get { return new RelayCommand<string>(ExecuteRemoveJobAttributeCommand, EvaluateRemoveJobAttributeCommand); } }


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
        /// JobAttributes 
        /// </summary>
        public ObservableCollection<string> JobAttributes 
        { 
            get => lstJobAttributes;
            set
            {
                lstJobAttributes = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// SelectedJobAttribute 
        /// 
        /// The job attribute selected by the user
        /// </summary>
        public string SelectedJobAttribute 
        { 
            get => m_sSelectedJobAttribute;
            set
            {
                m_sSelectedJobAttribute = value;
                OnNotifyPropertyChanged();
            }
        }

        /// <summary>
        /// NewJobAttribute
        /// 
        /// Bound to the text box at top of job attribute
        /// listbox, this is the new job attribute added by the user.
        /// </summary>
        public string NewJobAttribute 
        { 
            get => m_sNewJobAttribute;
            set
            {
                m_sNewJobAttribute = value;
                OnNotifyPropertyChanged();
            }
        }

        public void OnLoaded(object sender, EventArgs e)
        {
            try
            {
                JobAttributes.Clear();
                JobAttributes =_jale.LoadFromFile();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Unable to retrieve print job attributes from file, reason: " + ex.Message, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void OnClose(object sender, EventArgs e)
        {
            MyWindow.Close();
        }

        public void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                _jale.SaveToFile(JobAttributes);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to save print job attributes to file, reason: " + ex.Message, Defines.APP_NAME, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool EvaluateExitCommand()
        {
            return true;
        }

        private void ExecuteExitCommand()
        {
            OnClose(this, null);
        }

        private bool EvaluateAddJobAttributeCommand()
        {
            return NewJobAttribute != null;
        }

        private void ExecuteAddJobAttributeCommand()
        {
            JobAttributes.Add(NewJobAttribute);
        }

        private bool EvaluateRemoveJobAttributeCommand(string sAttribute)
        {
            return sAttribute != null;
        }

        private void ExecuteRemoveJobAttributeCommand(string sAttribute)
        {
            JobAttributes.Remove(sAttribute);
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
