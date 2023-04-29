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
using ExpandScadaEditor.ScreenEditor.Items;

namespace ExpandScadaEditor.ScreenEditor.WorkspaceHelperControls
{
    /// <summary>
    /// Логика взаимодействия для ElementsSelectedBorder.xaml
    /// </summary>
    public partial class ElementsSelectedBorder : UserControl
    {
        public ElementsSelectedBorder()
        {
            InitializeComponent();
        }

        public void AddBorderOnWorkspace(string name, Dictionary<string, ScreenElement> selectedElements, Canvas workspace)
        {
            // calculate coordinates of all elements and find position and size for this border







            //this.Name = name;
            //this.Width = element.ActualWidth + 1;
            //this.Height = element.ActualHeight + 1;
            //workspace.Children.Add(this);
            //Canvas.SetLeft(this, element.CoordX - 1);
            //Canvas.SetTop(this, element.CoordY - 1);
        }
    }
}
