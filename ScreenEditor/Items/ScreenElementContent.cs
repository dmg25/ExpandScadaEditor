using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ExpandScadaEditor.ScreenEditor.Items.Properties;

namespace ExpandScadaEditor.ScreenEditor.Items
{
    public class ScreenElementContent : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<GroupOfProperties> elementPropertyGroups = new ObservableCollection<GroupOfProperties>();
        public ObservableCollection<GroupOfProperties> ElementPropertyGroups
        {
            get
            {
                return elementPropertyGroups;
            }
            set
            {
                elementPropertyGroups = value;
            }
        }

        public ScreenElementContent()
        {
            CreateEditableProperties();
        }

        private void CreateEditableProperties()
        {
            GroupOfProperties newGroup = new GroupOfProperties("ContentCommon", new ObservableCollection<ElementProperty>()
            {
                ScreenElement.CreateEditableDependencyProperty<double>(nameof(Opacity), "Opacity", this, (val) =>
                {
                    if (val < 0 || val > 1)
                    {
                        return "Value must be between 0 and 1";
                    }
                    return string.Empty;
                }),
            });

            ElementPropertyGroups.Add(newGroup);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

    }
}
