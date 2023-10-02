using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ExpandScadaEditor.ScreenEditor.Items.Properties
{
    /// <summary>
    /// Логика взаимодействия для PropertySignalSelection.xaml
    /// </summary>
    public partial class PropertySignalSelection : Window
    {
        /*  - Do we care which screenElement is it?
         *  - Just be sure, that if we delete element - signal will get it -> -1 listener/writer
         *  - 
         * 
         * 
         * 
         * */

        protected PropertySignalSelectionVM VM
        {
            get { return (PropertySignalSelectionVM)Resources["ViewModel"]; }
        }

        public PropertySignalSelection(ElementProperty property)
        {
            InitializeComponent();
            VM.Initialize(property);
        }
    }
}
