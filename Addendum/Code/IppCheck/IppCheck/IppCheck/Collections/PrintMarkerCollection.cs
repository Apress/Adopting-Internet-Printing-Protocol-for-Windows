using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using MicroMvvm;
using System.Collections.ObjectModel;
using System.Windows;
using Org.BouncyCastle.Operators.Utilities;

namespace IppCheck
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

        /// <summary>
        /// AddMarker
        /// 
        /// ObservableCollection is not thread-safe, and any updates to 
        /// it must be made on the UI thread. To get around this I used the
        /// Dispatcher.Invoke method to marshall back to the Dispatcher thread 
        /// when modifying the collection. Ugly and slow, but effective...:-)
        /// </summary>
        /// <param name="marker"></param>
        public void AddMarker(PrintMarker marker) 
        { 
            Application.Current.Dispatcher.Invoke(() =>
            {
                PrintMarkers.Add(marker);
            });
        }

        /// <summary>
        /// Find
        /// 
        /// 
        /// Find a marker object in the collection
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
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
