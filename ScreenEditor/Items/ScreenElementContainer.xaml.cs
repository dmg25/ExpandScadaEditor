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
using ExpandScadaEditor.ScreenEditor.WorkspaceHelperControls;

namespace ExpandScadaEditor.ScreenEditor.Items
{
    /// <summary>
    /// Логика взаимодействия для ScreenElementContainer.xaml
    /// </summary>
    public partial class ScreenElementContainer : ScreenElement
    {
        public ScreenElementContainer(ScreenElement element)
        {
            InitializeComponent();

            RootContainer.Children.Add(element);

            RootContainer.Children.Add(new ElementResizingBorder() { Name = "RESIZE_BORDER" });
            RootContainer.Children.Add(new ElementMovingBorder() { Name = "MOVING_BORDER" });
            RootContainer.Children.Add(new MouseOverElementBorder() { Name = "COVER_BORDER" });



        }
    }
}
