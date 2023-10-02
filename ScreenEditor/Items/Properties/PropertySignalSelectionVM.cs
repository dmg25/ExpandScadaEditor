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
                        Property.ConnectedSignal = new Signal<int>(8888, "Dummy", "Stupid test");
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
