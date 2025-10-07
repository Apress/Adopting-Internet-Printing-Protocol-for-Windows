using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using MicroMvvm;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Media;
using iText.IO.Image;
using System.Reflection;
using CsIppRequestLib;

namespace IppCheck
{
    public class IppAttributeCollection : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<IppAttribute> _printer_attributes { get; set; }


        public IppAttributeCollection()
        {
            _printer_attributes = new ObservableCollection<IppAttribute>();
            OnNotifyPropertyChanged("Printer_Attributes");
        }

        public ObservableCollection<IppAttribute> Printer_Attributes
        {
            get => _printer_attributes;
            set
            {
                _printer_attributes = value;
                OnNotifyPropertyChanged();
            }
        }

        public bool Add(IppAttribute pa)
        {
            IppAttribute _pa = Find(pa.Name);
            if (_pa == null)
            {
                _printer_attributes.Add(pa);
                return true;
            }
            else
            {
                return false;
            }
        }

        
        public void CollectionUpdated()
        {
            OnNotifyPropertyChanged("Printer_Attributes");
        }

        public void Clear()
        {
            _printer_attributes.Clear();
        }

        /// <summary>
        /// GetAttributeValueString
        /// 
        /// Given a printer attribute, this method returns a string 
        /// attribute value or NULL.
        /// </summary>
        /// <param name="attribute_name"></param>
        /// <returns></returns>
        public string GetAttributeValueString(string attribute_name)
        {
            IppAttribute pa = Find(attribute_name);
            if(pa != null)
            {
                int len = pa.AttributeValues.Count;
                string valString = "";
                for(int i=0; i<len; i++)
                {
                    if (i < (len - 1))
                        valString += pa.AttributeValues[i].ToString() + ",";
                    else
                        valString += pa.AttributeValues[i].ToString();
                }
                return valString;
            }
            return null;
        }

        public IppAttribute Find(string attribute_name)
        {
            return _printer_attributes.FirstOrDefault(x => String.Equals(x.Name, attribute_name, StringComparison.CurrentCultureIgnoreCase));
        }

        private void OnNotifyPropertyChanged([CallerMemberName] string sPropertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(sPropertyName));
        }
    }
}
