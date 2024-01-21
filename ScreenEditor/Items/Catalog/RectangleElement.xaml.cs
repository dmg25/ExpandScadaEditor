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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExpandScadaEditor.ScreenEditor.Items.Catalog
{
    /// <summary>
    /// Логика взаимодействия для RectangleElement.xaml
    /// </summary>
    public partial class RectangleElement : ScreenElementContent
    {
        public RectangleElement(): base(nameof(RectangleElement))
        {
            InitializeComponent();
        }
    }
}
