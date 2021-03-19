using PicSimulatorLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace PicSimulator
{
    public abstract class IOControl : UserControl, INotifyPropertyChanged
    {
        private IO _link;
        public IO link
        {
            get => _link;
            set
            {
                if (_link != null)
                    _link.reference.ValueChanged -= Reference_ValueChanged;

                _link = value;
                _link.reference.ValueChanged += Reference_ValueChanged;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("State"));
            }
        }

        public bool State { get => link?.GetValue() == true; private set { } }

        public event PropertyChangedEventHandler PropertyChanged;

        public IOControl()
        {
            DataContext = this;
        }

        int i = 0;
        private void Reference_ValueChanged(object sender, RegisterValueChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("State"));
        }
    }
}
