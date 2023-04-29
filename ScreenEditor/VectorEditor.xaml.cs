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

using ExpandScadaEditor.ScreenEditor.WorkspaceHelperControls;

namespace ExpandScadaEditor.ScreenEditor
{
    /*          EDITOR'S OPERATIONS
     *      
     *      MOVE ELEMENT
     *      
     *      MOVING WITH COPYING
     *      
     *      SELECT ONE ELEMENT AND SHOW BORDER FOR RESIZING
     *          - create list of selected elements, add one or many elements on selecting and clear after dropping selection
     *          - each element in this list must show very thin border (just by property on basic element class)
     *          - every time recalculate rectangle around these elements and show border around this invisible rectangle
     *          - show on this rectangle boxes for resizing - only 4 on the angles
     *          - change cursor if it is over border or one of boxes. Add also special case - on the side of box for rotation
     *          - thickness of each border must be equal on any zoom variant
     *          - show thin border border on mouseOver event and hide on mouseGone
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

    public enum MouseMovingMode
    {
        None,
        MoveSelectedElements,
        Selecting,

    }



    /// <summary>
    /// Логика взаимодействия для VectorEditor.xaml
    /// </summary>
    public partial class VectorEditor : UserControl
    {
        //const double BORDER_OFFSET = 5d;

        const string MOUSE_OVER_SELECTED = "MOUSE_OVER_SELECTED";

        MouseMovingMode CurrentMouseMovingMode = MouseMovingMode.None;

        protected VectorEditorVM ViewModel
        {
            get { return (VectorEditorVM)Resources["ViewModel"]; }
        }

        Dictionary<string, ScreenElement> elementsOnWorkSpace = new Dictionary<string, ScreenElement>();
        ScreenElement SelectedElement { get; set; }

        Dictionary<string, ScreenElement> SelectedElements = new Dictionary<string, ScreenElement>();

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

                // Just for showing border on mouse moving
                pair.Value.MouseEnter += Element_MouseEnter;
                pair.Value.MouseLeave += Element_MouseLeave;
            }

            

        }

        private void Element_MouseLeave(object sender, MouseEventArgs e)
        {
            var element = sender as ScreenElement;

            FrameworkElement borderOverSelected = WorkSpace.Children.Cast<FrameworkElement>().Where(x => x.Name == $"{element.Name}_{MOUSE_OVER_SELECTED}").First();

            if (borderOverSelected != null)
            {
                WorkSpace.Children.Remove(borderOverSelected);
                borderOverSelected = null;
            }

        }

        private void Element_MouseEnter(object sender, MouseEventArgs e)
        {
            // Add the border
            var element = sender as ScreenElement;

            // If element already selected, border must be shown already
            if (!SelectedElements.ContainsKey(element.Name))
            {
                MouseOverElementBorder borderAround = new MouseOverElementBorder();
                borderAround.AddBorderOnWorkspace($"{element.Name}_{MOUSE_OVER_SELECTED}", element, WorkSpace);
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




            if (SelectedElement != null && e.LeftButton == MouseButtonState.Pressed && elementsOnWorkSpace.ContainsKey(SelectedElement.Name))
            {
                CurrentMouseMovingMode = MouseMovingMode.MoveSelectedElements;
            }
            else
            {
                CurrentMouseMovingMode = MouseMovingMode.None;
            }


            MouseMovingEffects(mousePosition);
        }




        void MouseMovingEffects(Point currentPosition)
        {
            switch (CurrentMouseMovingMode)
            {
                case MouseMovingMode.MoveSelectedElements:
                    double newPositionX = currentPosition.X - SelectedElementMousePressedCoordX;
                    double newPositionY = currentPosition.Y - SelectedElementMousePressedCoordY;

                    // MAKE LOGIC FOR MANY ELEMENTS MOVING


                    // check the borders of the workspace
                    if (currentPosition.X >= WorkSpace.ActualWidth || currentPosition.X <= 0
                       || currentPosition.Y >= WorkSpace.ActualHeight || currentPosition.Y <= 0)
                    {
                        break;
                    }

                    // move element
                    SelectedElement.CoordX = newPositionX;
                    SelectedElement.CoordY = newPositionY;
                    Canvas.SetLeft(SelectedElement, newPositionX);
                    Canvas.SetTop(SelectedElement, newPositionY);
                    break;

                //case MouseMovingMode.MoveSelectedElements:
                //    double newPositionX = currentPosition.X - SelectedElementMousePressedCoordX;
                //    double newPositionY = currentPosition.Y - SelectedElementMousePressedCoordY;

                //    // check the borders of the workspace
                //    if (currentPosition.X >= WorkSpace.ActualWidth || currentPosition.X <= 0
                //       || currentPosition.Y >= WorkSpace.ActualHeight || currentPosition.Y <= 0)
                //    {
                //        break;
                //    }

                //    // move element
                //    SelectedElement.CoordX = newPositionX;
                //    SelectedElement.CoordY = newPositionY;
                //    Canvas.SetLeft(SelectedElement, newPositionX);
                //    Canvas.SetTop(SelectedElement, newPositionY);
                //    break;



            }






        }



        void SelectNewElement(ScreenElement element)
        {
            // add to the dictionaly and add border around
        }



        void DeselectOneElement(ScreenElement element)
        {
            // find border of this element and delete it
            // remove this element from the list
            // create version for string as argument?
        }


        void DeselectAllElements()
        {
            // find and delete all borders for each element and clean the dictionary
        }

    }
}
