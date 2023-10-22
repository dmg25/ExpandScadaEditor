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
        public ScreenElementContainer(ScreenElementContent contentElement/*, double workspaceWidth, double workspaceHeight*/) 
            : base(contentElement/*, workspaceWidth, workspaceHeight*/)
        {
            InitializeComponent();

            var newItem = (ScreenElementContent)Activator.CreateInstance(contentElement.GetType());

            //RootContainer.Children.Add(newItem);

            RootContainer.Children.Insert(0, newItem);



            //RootContainer.Children.Add(new ElementResizingBorder() { Name = "RESIZE_BORDER" });
            //RootContainer.Children.Add(new ElementMovingBorder() { Name = "MOVING_BORDER" });
            //RootContainer.Children.Add(new MouseOverElementBorder() { Name = "COVER_BORDER" });

            // There is a problem - after it shown, I see all helpers bove element and not all events work. 
            // Manipulate with it somehow - e.g. return XAML version of helpers and add content somehow under them
            //IT WIRKS, now add only this content unter all of them and fix Rectangle - it is still stupid

            //base.InitializeItself();

        }
    }
}
