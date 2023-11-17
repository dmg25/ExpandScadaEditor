using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Common.Gateway;

namespace ExpandScadaEditor.ScreenEditor.Items.Properties
{
    public class PropertySignalSelectionVM : INotifyPropertyChanged
    {

        private ElementProperty property;
        public ElementProperty Property
        {
            get
            {
                return property;
            }
            set
            {
                property = value;
                NotifyPropertyChanged();
            }
        }

        private string signalName;
        public string SignalName
        {
            get
            {
                return signalName;
            }
            set
            {
                signalName = value;
                NotifyPropertyChanged();
            }
        }

        private int id;
        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                NotifyPropertyChanged();
            }
        }

        public void Initialize(ElementProperty property)
        {
            Property = property;
        }

        private Command _createSignal;
        public Command CreateSignal
        {
            get
            {
                return _createSignal ??
                    (_createSignal = new Command(obj =>
                    {
                        var signal = new Signal<int>();
                        signal.id = ID;
                        signal.name = SignalName;
                        Property.ConnectedSignal = signal;
                    },
                    obj =>
                    {
                        return true;
                    }));
            }
        }



        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
