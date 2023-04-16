using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Markup;
using System.Collections.ObjectModel;

namespace ExpandScadaEditor.ScreenEditor
{
    /*          EDITOR'S OPERATIONS
     *      
     *      MOVE ELEMENT
     *      
     *      MOVING WITH COPYING
     *      
     *      SELECT ONE ELEMENT AND SHOW BORDER FOR RESIZING
     *      
     *      SELECT GROUP OF ELEMENTS WITH MOUSE MOVING
     *      
     *      SELECT GROUP OF ELEMENTS WITH MOUSE POINTING
     *      
     *      MOVE GROUP OF ELEMENTS
     *      
     *      RESIZE GROUP OF ELEMENTS ???
     *      
     *      MOVING WITH COPYING FOR GROUP OF ELEMENTS
     *      
     *      ADD SCROLLBARS IF WORKSPACE BIGGER THEN WINDOW
     *      
     *      MOVE WORKSPACE WITH SCROLL OPERATIONS BY MOUSE + KEYS
     *      
     *      SHOW EMPTYNESS IF WINDOW IS BIGGER THEN WORKSPACE
     *      
     *      MOVING OPERATIONS ONLY INSIDE OF THE REAL WORKSPACE
     *      
     *      SCROLL WORKSPACE DURING THE MOVING
     *      
     *      CREATE ZOOM FUNCTIONS: TOOLS/MOUSE WHEEL + CTRL...
     *      
     *      
     *      
     *      COPY/PASTE WITH ADDITIONAL TOOLS/CONTEXT MENU
     *      
     *      
     * 
     * 
     * 
     * */



    /// <summary>
    /// Логика взаимодействия для VectorEditor.xaml
    /// </summary>
    public partial class VectorEditor : UserControl
    {
        //const double BORDER_OFFSET = 5d;

        protected VectorEditorVM ViewModel
        {
            get { return (VectorEditorVM)Resources["ViewModel"]; }
        }

        Dictionary<string, ScreenElement> elementsOnWorkSpace = new Dictionary<string, ScreenElement>();
        ScreenElement SelectedElement { get; set; }
        double SelectedElementMousePressedCoordX { get; set; }
        double SelectedElementMousePressedCoordY { get; set; }

        public VectorEditor()
        {
            InitializeComponent();


            // TODO For tests, later make it better
            ItemsTemplateSelector itemsTemplateSelector = (ItemsTemplateSelector)Resources["ItemsTemplateSelector"];
            Dictionary<string, DataTemplate> previewTemplates = new Dictionary<string, DataTemplate>
            {
                {nameof(TestItem1VM), CreateTemplateByName(typeof(TestItem1)) },
                {nameof(TestItem2VM), CreateTemplateByName(typeof(TestItem2)) }
            };
            itemsTemplateSelector.previewTemplates = previewTemplates;


            // Create/Load elements VM must be created automatically for each
            // TODO move it to loading process or smth
            elementsOnWorkSpace.Add("first", new TestItem2() { CoordX = 10, CoordY = 20, Name = "first"});



            // Put all elements on the workplace
            foreach (var pair in elementsOnWorkSpace)
            {
                WorkSpace.Children.Add(pair.Value);
                Canvas.SetLeft(pair.Value, pair.Value.CoordX);
                Canvas.SetTop(pair.Value, pair.Value.CoordY);
                pair.Value.PreviewMouseLeftButtonDown += Element_PreviewMouseLeftButtonDown;
                pair.Value.PreviewMouseLeftButtonUp += Element_PreviewMouseLeftButtonUp;
            }

            

        }

        private void Element_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectedElement = sender as ScreenElement;
            var mousePosition = e.GetPosition(SelectedElement);

            SelectedElementMousePressedCoordX = mousePosition.X;
            SelectedElementMousePressedCoordY = mousePosition.Y;
        }

        private void Element_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectedElement = null;
        }

        DataTemplate CreateTemplateByName(Type viewType)
        {
            string xamlTemplate = $"<DataTemplate> <items:{viewType.Name}/> </DataTemplate>";

            // What can we do simpler in this context?
            var context = new ParserContext();
            context.XamlTypeMapper = new XamlTypeMapper(new string[0]);
            context.XamlTypeMapper.AddMappingProcessingInstruction("items", viewType.Namespace, viewType.Assembly.FullName);

            context.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            context.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            context.XmlnsDictionary.Add("items", "items");

            var template = (DataTemplate)XamlReader.Parse(xamlTemplate, context);
            return template;
        }


       






        private void WorkSpace_MouseMove(object sender, MouseEventArgs e)
        {
            // test
            var mousePosition = e.GetPosition(WorkSpace);
            ViewModel.MouseX = mousePosition.X;
            ViewModel.MouseY = mousePosition.Y;




            if (SelectedElement != null && e.LeftButton == MouseButtonState.Pressed)
            {
                if (elementsOnWorkSpace.ContainsKey(SelectedElement.Name))
                {
                    double newPositionX = mousePosition.X - SelectedElementMousePressedCoordX;
                    double newPositionY = mousePosition.Y - SelectedElementMousePressedCoordY;

                    // check the borders of the workspace
                    if (mousePosition.X >= WorkSpace.ActualWidth || mousePosition.X <= 0
                       || mousePosition.Y >= WorkSpace.ActualHeight || mousePosition.Y <= 0)
                    {
                        return;
                    }

                    // move element
                    SelectedElement.CoordX = newPositionX;
                    SelectedElement.CoordY = newPositionY;
                    Canvas.SetLeft(SelectedElement, newPositionX);
                    Canvas.SetTop(SelectedElement, newPositionY);
                }

            }

        }

    }
}
