using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


namespace CS_GPA_CH3_WPF
{
    public interface IViewModel 
    {
        Window MyWindow { get; set; }
        void OnLoaded(object sender, EventArgs e);
        void OnClose(object sender, EventArgs e);
        void OnClosing(object sender, System.ComponentModel.CancelEventArgs e);
    }
}
