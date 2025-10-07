using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.IO;
using System.Collections.ObjectModel;

namespace IppCheck
{
    public sealed class JobAttributesListEditor : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string jobAttributesFile;
        private static readonly JobAttributesListEditor instance = new JobAttributesListEditor();

        static JobAttributesListEditor()
        {
        }

        private JobAttributesListEditor()
        {
        }

        public static JobAttributesListEditor GetInstance
        {
            get
            {
                return instance;
            }
        }

        /// <summary>
        ///  LoadFromFile
        ///  
        /// Load job attributes from backing file
        /// </summary>
        /// <exception cref="Exception"></exception>
        public ObservableCollection<string> LoadFromFile()
        {
            ObservableCollection<string> _jobAttributes = new ObservableCollection<string>();
            try
            {
                string[] lines = File.ReadAllLines(JobAttributesListFile);
                foreach (string line in lines)
                {
                    if (!String.IsNullOrWhiteSpace(line))
                    {
                        _jobAttributes.Add(line.Trim());
                    }
                }
                return _jobAttributes;  
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load job attributes from file {JobAttributesListFile}, reason: {ex.Message}");
            }
        }

        /// <summary>
        /// SaveToFile
        /// 
        /// Save job attributes to backing file
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void SaveToFile(ObservableCollection<string> _jobAttributes)
        {
            try
            {
                string[] lines = _jobAttributes.ToArray();
                File.WriteAllLines(JobAttributesListFile, lines);
            }
            catch(Exception ex)
            {
                throw new Exception($"Failed to save job attributes to file {JobAttributesListFile}, reason: {ex.Message}");
            }
        }

        /// <summary>
        /// JobAttributesListFile
        /// 
        /// The job attributes list backing file
        /// </summary>
        public string JobAttributesListFile
        {
            get => jobAttributesFile;
            set
            {
                jobAttributesFile = value;
                OnNotifyPropertyChanged();
            }
        }

        private void OnNotifyPropertyChanged([CallerMemberName] string sPropertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(sPropertyName));
        }
    }
}
