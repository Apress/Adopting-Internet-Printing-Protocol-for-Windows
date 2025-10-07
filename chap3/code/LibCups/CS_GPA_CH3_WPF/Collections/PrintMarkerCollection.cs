using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using MicroMvvm;
using System.Collections.ObjectModel;

namespace CS_GPA_CH3_WPF
{
    public class PrintMarkerCollection : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private ObservableCollection<PrintMarker> _printmarkers;

        public PrintMarkerCollection()
        {
            _printmarkers = new ObservableCollection<PrintMarker>();
        }

        public ObservableCollection<PrintMarker> PrintMarkers 
        { 
            get => _printmarkers;
            set
            {
                _printmarkers = value;
                OnNotifyPropertyChanged();
            }
        }

        public void AddMarker(PrintMarker marker) 
        { 
            PrintMarkers.Add(marker);   
        }

        public PrintMarker Find(string name)
        {
            return PrintMarkers.FirstOrDefault(x => String.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }
        private void OnNotifyPropertyChanged([CallerMemberName] string sPropertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(sPropertyName));
        }
    }
}
