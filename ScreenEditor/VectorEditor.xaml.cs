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
     *   +++MOVE ELEMENT
     *          - press on element - it will be selected. And existed selection must be dropped before
     *              - but only if there was no moving between mouseDown and mouseUp
     *          - If was mouseDown on selected element/s and moving after - move all elements and do not drop selection
     *              - care about borders of workspace - do not let any element go out of border
     *              - check if pressed element is already selected and move all elements in this case
     *              - if was not selected - drop current selection and select it and move only this element
     *          - 
     *      
     *      COPYING & PASTE
     *          - Copy by command
     *              - r-click on screen element shows context menu (comes from basic class)
     *              - there is copy command - press - this item(s) (SELECTED) will be saved to buffer
     *              - the same must be for any selected group from menu/toolbox
     *              - catch ctrl+c command - do the same
     *          - Copy during the moving
     *              - if user select the group, and started to move it, then:
     *                  - if ctrl not pressed - move items as usual
     *                  - if pressed - place original element on starting position and create a copy with opacity, move the copy
     *                  - if element was moved and only then ctrl was pressed - return original element and move the copy
     *                  - if moving happens with ctrl and ctrl was dropped - delete copy of element and move original element to current position
     *          - Paste by command (context menu on workspace or menu/toolbox, ctrl+v)
     *              - each element in buffer has current coordinates on copying moment - add to these coordinates 5+10px and place all elements with new coodrs
     *              - give each element new name. 
     *                  - if current name has no number postfix (_NN) there is not, ot can not be converted to number - add postfix "_1"
     *                  - if there is postfix - increase it
     *              - check if pasting coordinates are not out of border. If new coord bigger then border - set elements at border
     *              
     *              
     *              
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
     *   +++SELECT GROUP OF ELEMENTS WITH MOUSE MOVING
     *          - create selection border during mouse moving
     *          - select right-down: object selected if whole object covered
     *          - select left-down:  object selected if whole object covered
     *          - select left-up:    object selected if at least one point of it covered 
     *          - select right-up:   object selected if at least one point of it covered 
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
     *      UNDO/REDO user's action
     *      
     *      ADD ICON FOR ROTATION
     *      
     *      COPY/PASTE WITH ADDITIONAL TOOLS/CONTEXT MENU
     *      
     *      
     * 
     * 
     * 
     * */

    /*  TODO FUNCTIONALITY
     *  
     *  1. When a group of elements selected show special icon "+" with arrows, to move whole group. 
     *     There could be situation, that selected elements are too small to move them by pressing on them
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

        //const string MOUSE_OVER_SELECTED = "MOUSE_OVER_SELECTED";
        const string SELECTING_RECTANGLE = "SELECTING_RECTANGLE";
        //const string SELECTED_RECTANGLE = "SELECTED_RECTANGLE";

        MouseMovingMode CurrentMouseMovingMode = MouseMovingMode.None;

        protected VectorEditorVM ViewModel
        {
            get { return (VectorEditorVM)Resources["ViewModel"]; }
        }

        Dictionary<string, ScreenElement> elementsOnWorkSpace = new Dictionary<string, ScreenElement>();
        //ScreenElement SelectedElement { get; set; }

        //Dictionary<string, ScreenElement> SelectedElements = new Dictionary<string, ScreenElement>();

        List<ScreenElement> SelectedElements = new List<ScreenElement>();
        int selectedElementIndexByMouse = -1;

        double SelectedElementMousePressedCoordX { get; set; }
        double SelectedElementMousePressedCoordY { get; set; }
        bool elementsWereMoved = false;

        //bool movingInResizeMode = false;

        ElementsSelectingBorder borderSelecting;

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
            elementsOnWorkSpace.Add("second", new TestItem2() { CoordX = 100, CoordY = 100, Name = "second" });
            elementsOnWorkSpace.Add("third", new TestItem2() { CoordX = 100, CoordY = 200, Name = "third" });


            // Put all elements on the workplace
            foreach (var pair in elementsOnWorkSpace)
            {
                WorkSpace.Children.Add(pair.Value);
                Canvas.SetLeft(pair.Value, pair.Value.CoordX);
                Canvas.SetTop(pair.Value, pair.Value.CoordY);
                pair.Value.MouseLeftButtonDown += Element_MouseLeftButtonDown;
                pair.Value.MouseLeftButtonUp += Element_MouseLeftButtonUp;

                pair.Value.OnElementResizing += Element_OnElementResizing;
                //pair.Value.StartResizing += Element_StartResizing;
                //pair.Value.StopResizing += Element_StopResizing;
            }
            WorkSpace.MouseLeftButtonDown += WorkSpace_MouseLeftButtonDown;
            WorkSpace.MouseLeftButtonUp += WorkSpace_MouseLeftButtonUp;

        }

        private void Element_OnElementResizing(object sender, ResizingEventArgs e)
        {
            var element = sender as ScreenElement;
            switch (e.ResizingType)
            {
                case ResizingType.ChangeSize:
                    if (!double.IsNaN(e.NewWidth) && e.NewWidth + element.CoordX < WorkSpace.ActualWidth)
                    {
                        element.Width = e.NewWidth;
                    }
                    if (!double.IsNaN(e.NewHeight) && e.NewHeight + element.CoordY < WorkSpace.ActualHeight)
                    {
                        element.Height = e.NewHeight;
                    }
                    Canvas.SetLeft(element, element.CoordX);
                    Canvas.SetTop(element, element.CoordY);
                    break;
                    
            }


        }

        private void WorkSpace_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // end of selecting by mouse
            if (borderSelecting != null)
            {
                WorkSpace.Children.Remove(borderSelecting);
                borderSelecting = null;
            }
            WorkSpace.ReleaseMouseCapture();
        }

        private void WorkSpace_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Here is a logic for Canvas event only. If we clicked on any element - igrore this event
            if (e.OriginalSource is not Canvas)
            {
                return;
            }

            // start of selecting by mouse
            var mousePosition = e.GetPosition(WorkSpace);

            // drop all selected elements if there were before
            DeselectAllElements();

            borderSelecting = new ElementsSelectingBorder();
            borderSelecting.AddBorderOnWorkspace(SELECTING_RECTANGLE, WorkSpace, mousePosition);
            WorkSpace.CaptureMouse();

        }

        private void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // if this element is not selected - drop current selection and select this element
            // if this element is selected - do nothing

            var element = sender as ScreenElement;

            if (!SelectedElements.Contains(element))
            {
                DeselectAllElements();
                SelectElement(element);
            }

            //SelectedElement = sender as ScreenElement;
            var mousePosition = e.GetPosition(element);
            SelectedElementMousePressedCoordX = mousePosition.X;
            SelectedElementMousePressedCoordY = mousePosition.Y;
            selectedElementIndexByMouse = SelectedElements.IndexOf(element);

        }

        private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // check if there was a moving. If was - do nothing, if wasn't - drop current selection and select current element only
            var element = sender as ScreenElement;

            if (!elementsWereMoved)
            {
                DeselectAllElements();
                SelectElement(element);
            }

            elementsWereMoved = false;
            selectedElementIndexByMouse = -1;
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

            //if (movingInResizeMode)
            //{
            //    return;
            //}

            if (e.LeftButton == MouseButtonState.Pressed && borderSelecting != null)
            {
                CurrentMouseMovingMode = MouseMovingMode.Selecting;
            }
            else if(e.LeftButton == MouseButtonState.Pressed && SelectedElements.Count != 0 /*&& elementsOnWorkSpace.ContainsKey(SelectedElement.Name)*/)
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
                    elementsWereMoved = true;
                    double newPositionX = currentPosition.X - SelectedElementMousePressedCoordX;
                    double newPositionY = currentPosition.Y - SelectedElementMousePressedCoordY;

                    if (selectedElementIndexByMouse < 0)
                    {
                        return;
                    }

                    double offsetX = newPositionX - SelectedElements[selectedElementIndexByMouse].CoordX;
                    double offsetY = newPositionY - SelectedElements[selectedElementIndexByMouse].CoordY;

                    // calculate offset for pressed element for new coordinates
                    // use this offset for each selected element - calculate new coordinates
                    // set new coordinates for each element

                    // check for each element the border of workspace. If border reached - break
                    // we can move one element a little off the board, but if there is a group - nonono
                    if (SelectedElements.Count == 1)
                    {
                        if (currentPosition.X >= WorkSpace.ActualWidth || currentPosition.X <= 0
                           || currentPosition.Y >= WorkSpace.ActualHeight || currentPosition.Y <= 0)
                        {
                            return;
                        }
                    }
                    else
                    {
                        foreach (var element in SelectedElements)
                        {
                            if (element.CoordX + offsetX + element.ActualWidth >= WorkSpace.ActualWidth
                                || element.CoordY + offsetY + element.ActualHeight >= WorkSpace.ActualHeight
                                || element.CoordX + offsetX <= 0 || element.CoordY + offsetY <= 0)
                            {
                                return;
                            }
                        }
                    }


                    // set new coordinates for each element
                    foreach (var element in SelectedElements)
                    {
                        element.CoordX += offsetX;
                        element.CoordY += offsetY;
                        Canvas.SetLeft(element, element.CoordX);
                        Canvas.SetTop(element, element.CoordY);
                    }

                    break;
                case MouseMovingMode.Selecting:
                    if (borderSelecting != null)
                    {
                        // redraw border
                        borderSelecting.ContinueSelection(currentPosition);

                        // selecting 
                        ActualizeElementsCoveredBySelection();


                    }
                    break;



            }






        }



        void ActualizeElementsCoveredBySelection()
        {
            Point selectRect1 = new Point(borderSelecting.CoordX, borderSelecting.CoordY);
            Point selectRect2 = new Point(borderSelecting.CoordX + borderSelecting.ActualWidth, borderSelecting.CoordY + borderSelecting.ActualHeight);

            switch (borderSelecting.currentSelectingDirection)
            {
                case MouseSelectingDirection.RightDown:
                case MouseSelectingDirection.LeftDown:
                    // object selected if whole object covered
                    foreach (var element in elementsOnWorkSpace)
                    {
                        // check two points of current element: 1 - just coordinates; 2 - coord + width/height
                        // coordinate of 1 point must be more then coord of point of selection rect.
                        // coordinate of 2 point must be less then coord of second point of selection rect

                        Point elementRect1 = new Point(element.Value.CoordX, element.Value.CoordY);
                        Point elementRect2 = new Point(element.Value.CoordX + element.Value.ActualWidth, element.Value.CoordY + element.Value.ActualHeight);

                        if (selectRect1.X <= elementRect1.X && selectRect1.Y <= elementRect1.Y
                            && selectRect2.X >= elementRect2.X && selectRect2.Y >= elementRect2.Y)
                        {
                            if (!SelectedElements.Contains(element.Value))
                            {
                                SelectElement(element.Value);
                            }
                        }
                        else
                        {
                            if (SelectedElements.Contains(element.Value))
                            {
                                DeselectOneElement(element.Value);
                            }
                        }
                    }
                    break;
                case MouseSelectingDirection.LeftUp:
                case MouseSelectingDirection.RightUp:
                    // object selected in any piece of it is covered
                    foreach (var element in elementsOnWorkSpace)
                    {
                        // if any coordinate of any of 2 points intersected with coordinates of selecting rect - object selected

                        Point elementRect1 = new Point(element.Value.CoordX, element.Value.CoordY);
                        Point elementRect2 = new Point(element.Value.CoordX + element.Value.ActualWidth, element.Value.CoordY + element.Value.ActualHeight);

                        if (selectRect2.X >= elementRect1.X && selectRect2.Y >= elementRect1.Y &&
                            selectRect1.X <= elementRect2.X && selectRect1.Y <= elementRect2.Y)
                        {
                            if (!SelectedElements.Contains(element.Value))
                            {
                                SelectElement(element.Value);
                            }
                        }
                        else
                        {
                            if (SelectedElements.Contains(element.Value))
                            {
                                DeselectOneElement(element.Value);
                            }
                        }
                    }

                    break;
            }



        }



        void SelectElement(ScreenElement element)
        {
            // add to the dictionaly and add border around
            SelectedElements.Add(element);
            element.ShowResizeBorder();
        }



        void DeselectOneElement(ScreenElement element)
        {
            // find border of this element and delete it
            // remove this element from the list
            // create version for string as argument?

            if (!SelectedElements.Contains(element))
            {
                return;
            }

            SelectedElements.Remove(element);
            element.HideResizeBorder();
        }


        void DeselectAllElements()
        {
            // find and delete all borders for each element and clean the dictionary

            foreach (var element in SelectedElements)
            {
                element.HideResizeBorder();
            }

            SelectedElements.Clear();
        }

    }




}
