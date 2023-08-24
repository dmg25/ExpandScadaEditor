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
using System.Windows.Media.Media3D;

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
     *   +++COPYING & PASTE
     *          + Copy by command
     *          + Paste by command (context menu on workspace or menu/toolbox, ctrl+v)

     *              
     *   ++-(catalog's cursor problem)CREATE/DELETE ELEMENT     
     *          - Create element
     *              - User must press on the element on the catalog and move it to workspace
     *              - during the moving show opacitiezed element following the cursor
     *                  - We can crete new element and move it aside of cursor with opacity above the catalog
     *                  - then listen mouse up event on WP and if it was - create new element and finish operation
     *              - if user drop it on workspace - add element as new one on WP. If dropped to another place - ignore and delete
     *              
     *      
     *   +++SELECT ONE ELEMENT AND SHOW BORDER FOR RESIZING
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
     *   +++SELECT GROUP OF ELEMENTS WITH MOUSE POINTING
     *      
     *   +++MOVE GROUP OF ELEMENTS
     *            
     *   +++MOVING WITH COPYING FOR GROUP OF ELEMENTS
     *      
     *   +++ADD SCROLLBARS IF WORKSPACE BIGGER THEN WINDOW
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
     *   +++UNDO/REDO user's action
     *      
     *      ADD ICON FOR ROTATION
     *      
     *      COPY/PASTE WITH ADDITIONAL TOOLS/CONTEXT MENU
     *      
     *   +++SELECT ALL WITH CTRL+A
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

        //MouseMovingMode CurrentMouseMovingMode = MouseMovingMode.None;

        protected VectorEditorVM VM
        {
            get { return (VectorEditorVM)Resources["ViewModel"]; }
        }


        //ScreenElement SelectedElement { get; set; }

        //Dictionary<string, ScreenElement> SelectedElements = new Dictionary<string, ScreenElement>();

        List<(int id, double xCoord, double yCoord)> selectedPositionsBeforeMoving = new List<(int id, double xCoord, double yCoord)>();
        MouseMovingMode preMode = MouseMovingMode.None;
        ScreenElement tmpPreSelectedElement = new ScreenElement();
        int selectedElementIndexByMouse = -1;
        List<ScreenElement> TmpFollowerElements = new List<ScreenElement>();

        double SelectedElementMousePressedCoordX { get; set; }
        double SelectedElementMousePressedCoordY { get; set; }
        bool elementsWereMoved = false;

        ElementsSelectingBorder borderSelecting;

        ScreenElement elementFromCatalog;
        //ScreenElement tmpElementFromCatalog;
        bool elementFromCatalogMoved = false;

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


            VM.NewScreenElementAdded += VM_NewScreenElementAdded;
            VM.ScreenElementReplaced += VM_ScreenElementReplaced;
            VM.ScreenElementDeleted += VM_ScreenElementDeleted;
            VM.SelectedElementsDeleted += VM_SelectedElementsDeleted;
            VM.SelectTheseElements += VM_SelectTheseElements; 
            VM.Initialize();

            WorkSpace.MouseLeftButtonDown += WorkSpace_MouseLeftButtonDown;
            WorkSpace.MouseLeftButtonUp += WorkSpace_MouseLeftButtonUp;

            //this.KeyDown += WorkSpace_PreviewKeyDown;

            // Here we have to save original state of this workspace. Opened new or loaded - here must be first point
            VM.UndoRedo.BasicUserAction(VM.ElementsOnWorkSpace.Values.ToList());
            //VM.UndoRedo.NewUserAction(VM.ElementsOnWorkSpace.Values.ToList());

            // events for creating element 
            //ElementCatalog.PreviewMouseLeftButtonDown += ElementCatalog_MouseLeftButtonDown;
            ElementCatalog.MouseLeftButtonUp += ElementCatalog_MouseLeftButtonUp;
            ElementCatalog.MouseMove += ElementCatalog_MouseMove;

           
        }

        private void VM_SelectTheseElements(object sender, ScreenElementsEventArgs e)
        {
            DeselectAllElements();
            e.Elements.ForEach(x => SelectElement(x));
        }

        private void VM_SelectedElementsDeleted(object sender, EventArgs e)
        {
            var selectedElementsTmp = VM.SelectedElements.ToList();
            DeselectAllElements();
            selectedElementsTmp.ForEach(x => VM_ScreenElementDeleted(null, new ScreenElementEventArgs() { Element = x }));
        }

        //private void WorkSpace_PreviewKeyDown(object sender, KeyEventArgs e)
        //{
        //     delete this event and use button on tooltab and command
        //     delete element, create and check new user action
        //    if (e.Key == Key.Delete && SelectedElements.Count != 0)
        //    {
        //         SelectedElements.ForEach(x => VM_ScreenElementDeleted(null, new ScreenElementEventArgs() { Element = x }));
        //    }
        //}

        private void VM_ScreenElementDeleted(object sender, ScreenElementEventArgs e)
        {
            e.Element.MouseLeftButtonDown -= Element_MouseLeftButtonDown;
            e.Element.MouseLeftButtonUp -= Element_MouseLeftButtonUp;

            e.Element.OnElementResizing -= Element_OnElementResizing;
            e.Element.ElementSizeChanged -= Element_ElementResized;

            //WorkSpace.Children.Remove(WorkSpace.Children.fir);

            WorkSpace.Children.Remove(e.Element);
            e.Element = null;
        }

        private void ElementCatalog_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && elementFromCatalog is not null)
            {
                //TranslateTransform transform = new TranslateTransform();
                //transform.X = Mouse.GetPosition(WorkSpace).X;
                //transform.Y = Mouse.GetPosition(WorkSpace).Y;

                // !!! ATTENTION !!! we decide that here elementFromCatalog can be only one, so we will use only first element in follower tmp list

                Point point = new Point(Mouse.GetPosition(WorkSpace).X, Mouse.GetPosition(WorkSpace).Y);

                if (!elementFromCatalogMoved)
                {
                    elementFromCatalogMoved = true;
                    // create new element on the CANVAS layer and set opacity 0,5 and -1 ID
                    // we can set coordinates to catalog's element, because we've copied it
                    elementFromCatalog.CoordX = point.X;
                    elementFromCatalog.CoordY = point.Y;
                    CreateTmpElementsWithOpacity(new List<ScreenElement>() { elementFromCatalog });

                    ElementCatalog.Cursor = Cursors.SizeAll;
                    Cursor = Cursors.SizeAll;
                }
                else
                {
                    TmpFollowerElements[0].CoordX = point.X;
                    TmpFollowerElements[0].CoordY = point.Y;
                    Canvas.SetLeft(TmpFollowerElements[0], TmpFollowerElements[0].CoordX);
                    Canvas.SetTop(TmpFollowerElements[0], TmpFollowerElements[0].CoordY);
                }
            }
        }

        private void ElementCatalog_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (elementFromCatalog is not null)
            {
                if (elementFromCatalogMoved)
                {
                    elementFromCatalogMoved = false;
                    if (TmpFollowerElements[0].CoordX > 0 && TmpFollowerElements[0].CoordX < WorkSpace.ActualWidth
                    && TmpFollowerElements[0].CoordY > 0 && TmpFollowerElements[0].CoordY < WorkSpace.ActualHeight)
                    {
                        elementFromCatalog.CoordX = TmpFollowerElements[0].CoordX;
                        elementFromCatalog.CoordY = TmpFollowerElements[0].CoordY;
                        RemoveTmpElementsWithOpacity();
                    }
                    else
                    {
                        RemoveTmpElementsWithOpacity();
                        elementFromCatalog = null;
                        Cursor = Cursors.Arrow;
                        return;
                    }
                }
                else
                {
                    var position = FindPlaceForNewElement(elementFromCatalog);
                    elementFromCatalog.CoordX = position.x;
                    elementFromCatalog.CoordY = position.y;
                }

                VM.AddNewScreenElement(elementFromCatalog);
                VM.UndoRedo.NewUserAction(VM.ElementsOnWorkSpace, UndoRedoActionType.Create);
            }

            Cursor = Cursors.Arrow;
        }

        private void ListViewItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null)
            {
                // TODO this is only for cosmetics - solve this problem late
                 // One problem in this place.If I do some actions with element from catalog, then it looks like non - initialized, 
                 //it is kinda we here see right element, but on list shown some else.I can not use CatalogMode because of it and
                 //Cursor drops every time -looks not perfect.

                 //If you check dynamical tree in debugger - these properties crossed. why? check this.

                var element = item.Content as ScreenElement;
                //element.Cursor = Cursors.SizeAll;
                elementFromCatalog = (ScreenElement)Activator.CreateInstance(element.GetType());
                //element.CatalogMode = true;
            }
        }

        private void VM_ScreenElementReplaced(object sender, ReplacingElementEventArgs e)
        {
            if (e.OldElement != null)
            {
                e.OldElement.MouseLeftButtonDown -= Element_MouseLeftButtonDown;
                e.OldElement.MouseLeftButtonUp -= Element_MouseLeftButtonUp;
                e.OldElement.OnElementResizing -= Element_OnElementResizing;
                e.OldElement.ElementSizeChanged -= Element_ElementResized;
                WorkSpace.Children.Remove(e.OldElement);
                e.OldElement = null;
            }
            VM_NewScreenElementAdded(null, new ScreenElementEventArgs() { Element = e.NewElement });
        }

        private void VM_NewScreenElementAdded(object sender, ScreenElementEventArgs e)
        {
            WorkSpace.Children.Add(e.Element);
            Canvas.SetLeft(e.Element, e.Element.CoordX);
            Canvas.SetTop(e.Element, e.Element.CoordY);
            e.Element.MouseLeftButtonDown += Element_MouseLeftButtonDown;
            e.Element.MouseLeftButtonUp += Element_MouseLeftButtonUp;
            e.Element.OnElementResizing += Element_OnElementResizing;
            e.Element.ElementSizeChanged += Element_ElementResized;
        }

        private void Element_ElementResized(object sender, EventArgs e)
        {
            VM.UndoRedo.NewUserAction(VM.ElementsOnWorkSpace);
           // VM.UndoRedo.NewUserAction(sender as ScreenElement);
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

            //if (!VM.SelectedElements.Contains(element))
            if (VM.SelectedElements.Find(x => x.Id == element.Id) is null)
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    DeselectAllElements();
                }
                    
                SelectElement(element);
                tmpPreSelectedElement = element;
            }

            //SelectedElement = sender as ScreenElement;
            var mousePosition = e.GetPosition(element);
            SelectedElementMousePressedCoordX = mousePosition.X;
            SelectedElementMousePressedCoordY = mousePosition.Y;
            selectedElementIndexByMouse = VM.SelectedElements.IndexOf(element);

        }

        private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // check if there was a moving. If was - do nothing, if wasn't - drop current selection and select current element only
            var element = sender as ScreenElement;

            if (!elementsWereMoved)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (tmpPreSelectedElement is null)
                    {
                        DeselectOneElement(element);
                    }
                    else
                    {
                        if (VM.SelectedElements.Find(x => x.Id == element.Id) is null)
                        {
                            SelectElement(element);
                        } 
                    }
                }
                else
                {
                    DeselectAllElements();
                    if (VM.SelectedElements.Find(x => x.Id == element.Id) is null)
                    {
                        SelectElement(element);
                    }
                }
            }
            else
            {
                if (TmpFollowerElements.Count > 0)
                {
                    // elements were copied
                    var newElementsList = VM.CopyElementsInList(VM.SelectedElements);
                    for (int i = 0; i < newElementsList.Count; i++)
                    {
                        newElementsList[i].Name = null;
                        newElementsList[i].CoordX = TmpFollowerElements[i].CoordX;
                        newElementsList[i].CoordY = TmpFollowerElements[i].CoordY;
                        VM.AddNewScreenElement(newElementsList[i]);
                    }

                    VM.UndoRedo.NewUserAction(VM.ElementsOnWorkSpace, UndoRedoActionType.Create);
                    RemoveTmpElementsWithOpacity();
                    VM_SelectTheseElements(null, new ScreenElementsEventArgs() { Elements = newElementsList });
                }
                else
                {
                    // elements were moved
                    SelectedElementsToResizingMode();
                    // At the end of the moving - invoke new user action for undoredo
                    VM.UndoRedo.NewUserAction(VM.ElementsOnWorkSpace);
                }
                
            }

            elementsWereMoved = false;
            tmpPreSelectedElement = null;
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
            //--- test / delete later ---
            var mousePosition = e.GetPosition(WorkSpace);
            VM.MouseX = mousePosition.X;
            VM.MouseY = mousePosition.Y;
            //---------------------------
            MouseMovingMode currentMouseMovingMode = MouseMovingMode.None;

            if (e.LeftButton == MouseButtonState.Pressed && borderSelecting != null)
            {
                currentMouseMovingMode = MouseMovingMode.Selecting;
            }
            else if(e.LeftButton == MouseButtonState.Pressed && VM.SelectedElements.Count != 0)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    currentMouseMovingMode = MouseMovingMode.CopyDuringMoving;
                }
                else
                {
                    currentMouseMovingMode = MouseMovingMode.MoveSelectedElements;
                }
            }           

            MouseMovingEffects(mousePosition, currentMouseMovingMode);
        }

        void MouseMovingEffects(Point currentPosition, MouseMovingMode currentMovingMode)
        {
            if (VM.SelectedElements.Count > 0 && !elementsWereMoved)
            {
                VM.SelectedElements.ForEach(x => selectedPositionsBeforeMoving.Add((x.Id, x.CoordX, x.CoordY)));
            }

            switch (currentMovingMode)
            {
                case MouseMovingMode.MoveSelectedElements:
                    if (!elementsWereMoved)
                    {
                        SelectedElementsToMovingMode();
                        elementsWereMoved = true;
                    }

                    if (currentMovingMode != preMode)
                    {
                        RemoveTmpElementsWithOpacity();
                    }

                    ChangeCoorditanesOnMoving(VM.SelectedElements, currentPosition,
                        selectedElementIndexByMouse,
                        SelectedElementMousePressedCoordX, SelectedElementMousePressedCoordY);

                    break;
                case MouseMovingMode.CopyDuringMoving:
                    /*  put above first enterance and save start position of each selected element
                     *  use preMode here, update after each cycle in the bottom
                     *  if pre mode is different then 
                     *      - for moving - set all selected elements new coordinates and delete tmp elements if they are exist
                     *      - for copying - set all selected element old coordinates and create new elements with opacity, put them to tmp container
                     *  on mouse up event
                     *      - if tmp container is not empty - create new elements on these positions
                     *      - if empty - react like on regular moving
                     * */
                    if (!elementsWereMoved)
                    {
                        SelectedElementsToMovingMode();
                        elementsWereMoved = true;
                    }

                    if (currentMovingMode != preMode)
                    {
                        // create tmp followers
                        CreateTmpElementsWithOpacity(VM.SelectedElements);

                        //set all selected element old coordinates and create new elements with opacity, put them to tmp container
                        foreach (var element in VM.SelectedElements)
                        {
                            var coords = selectedPositionsBeforeMoving.Find(x => x.id == element.Id);
                            if (double.IsNaN(coords.xCoord) || double.IsNaN(coords.yCoord))
                            {
                                continue;
                            }
                            element.CoordX = coords.xCoord;
                            element.CoordY = coords.yCoord;
                            Canvas.SetLeft(element, element.CoordX);
                            Canvas.SetTop(element, element.CoordY);
                        }
                    }

                    ChangeCoorditanesOnMoving(TmpFollowerElements, currentPosition,
                        selectedElementIndexByMouse,
                        SelectedElementMousePressedCoordX, SelectedElementMousePressedCoordY);


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

            preMode = currentMovingMode;
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
                    foreach (var element in VM.ElementsOnWorkSpace)
                    {
                        // check two points of current element: 1 - just coordinates; 2 - coord + width/height
                        // coordinate of 1 point must be more then coord of point of selection rect.
                        // coordinate of 2 point must be less then coord of second point of selection rect

                        Point elementRect1 = new Point(element.Value.CoordX, element.Value.CoordY);
                        Point elementRect2 = new Point(element.Value.CoordX + element.Value.ActualWidth, element.Value.CoordY + element.Value.ActualHeight);

                        if (selectRect1.X <= elementRect1.X && selectRect1.Y <= elementRect1.Y
                            && selectRect2.X >= elementRect2.X && selectRect2.Y >= elementRect2.Y)
                        {
                            //if (!VM.SelectedElements.Contains(element.Value))
                            if (VM.SelectedElements.Find(x => x.Id == element.Value.Id) is null)
                            {
                                SelectElement(element.Value);
                            }
                        }
                        else
                        {
                            //if (VM.SelectedElements.Contains(element.Value))
                            if (VM.SelectedElements.Find(x => x.Id == element.Value.Id) is not null)
                            {
                                DeselectOneElement(element.Value);
                            }
                        }
                    }
                    break;
                case MouseSelectingDirection.LeftUp:
                case MouseSelectingDirection.RightUp:
                    // object selected in any piece of it is covered
                    foreach (var element in VM.ElementsOnWorkSpace)
                    {
                        // if any coordinate of any of 2 points intersected with coordinates of selecting rect - object selected

                        Point elementRect1 = new Point(element.Value.CoordX, element.Value.CoordY);
                        Point elementRect2 = new Point(element.Value.CoordX + element.Value.ActualWidth, element.Value.CoordY + element.Value.ActualHeight);

                        if (selectRect2.X >= elementRect1.X && selectRect2.Y >= elementRect1.Y &&
                            selectRect1.X <= elementRect2.X && selectRect1.Y <= elementRect2.Y)
                        {
                            //if (!VM.SelectedElements.Contains(element.Value))
                            if (VM.SelectedElements.Find(x => x.Id == element.Value.Id) is null)
                            {
                                SelectElement(element.Value);
                            }
                        }
                        else
                        {
                            //if (VM.SelectedElements.Contains(element.Value))
                            if (VM.SelectedElements.Find(x => x.Id == element.Value.Id) is not null)
                            {
                                DeselectOneElement(element.Value);
                            }
                        }
                    }

                    break;
            }



        }



        void SelectedElementsToMovingMode()
        {
            VM.SelectedElements.ForEach(x => x.HideResizeBorder());
            VM.SelectedElements.ForEach(x => x.ShowMovingBorder());
        }

        void SelectedElementsToResizingMode()
        {
            VM.SelectedElements.ForEach(x => x.HideMovingBorder());
            VM.SelectedElements.ForEach(x => x.ShowResizeBorder());
        }

        void SelectElement(ScreenElement element)
        {
            // add to the dictionaly and add border around
            VM.SelectedElements.Add(element);
            element.ShowResizeBorder();
        }



        void DeselectOneElement(ScreenElement element)
        {
            // find border of this element and delete it
            // remove this element from the list
            // create version for string as argument?

            //if (!VM.SelectedElements.Contains(element))
            if (VM.SelectedElements.Find(x => x.Id == element.Id) is null)
            {
                return;
            }

            VM.SelectedElements.Remove(element);
            element.HideResizeBorder();
        }


        void DeselectAllElements()
        {
            // find and delete all borders for each element and clean the dictionary

            foreach (var element in VM.SelectedElements)
            {
                element.HideResizeBorder();
            }

            VM.SelectedElements.Clear();
        }




        (double x, double y) FindPlaceForNewElement(ScreenElement element)
        {
            double centerX = WorkSpace.ActualWidth / 2;
            double centerY = WorkSpace.ActualHeight / 2;

            bool endX = false;
            bool endY = false;

            while (!endX || !endY)
            {
                var elementInCenter = VM.ElementsOnWorkSpace.Values.FirstOrDefault(x => x.CoordX >= centerX - 1 && x.CoordX <= centerX + 1 && x.CoordY >= centerY - 1 && x.CoordY <= centerY + 1);

                if (elementInCenter is null)
                {
                    return (centerX, centerY);
                }
                else
                {
                    double newCenterX = centerX + 10;
                    double newCenterY = centerY + 10;

                    if (newCenterX + element.Width >= WorkSpace.ActualWidth)
                    {
                        newCenterX = WorkSpace.ActualWidth - element.Width;
                        endX = true;
                    }

                    if (newCenterY + element.Height >= WorkSpace.ActualHeight)
                    {
                        newCenterY = WorkSpace.ActualHeight - element.Height;
                        endY = true;
                    }

                    centerX = newCenterX < 0 ? 0 : newCenterX;
                    centerY = newCenterY < 0 ? 0 : newCenterY;
                }
            }

            return (centerX, centerY);

        }



        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();

            // TODO when you add properties for workspace size and it will be initialized in VM - move it there!
            WorkSpace.Width = 500;
            WorkSpace.Height = 500;
            WorkSpace.Background = Brushes.LightGray;
            VM.WorkSpaceHeight = WorkSpace.ActualHeight;
            VM.WorkSpaceWidth = WorkSpace.ActualWidth;
        }


        // Catching hotheys. Standard way doesn't work at all (from XAML) by unknown reason. 
        // I make a focus, but InputBinding is not working. 
        // Looks like I have to use RoutedCommand, but for now it looks no good, so we do it here.
        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            ICommand command = null;

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                // Element resizing
                if (e.Key == Key.Right || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Up)
                {
                    switch (e.Key)
                    {
                        case Key.Right:
                            if (CanMoveElements(VM.SelectedElements, e.Key))
                            {
                                VM.SelectedElements.ForEach(x => x.Width++);
                            }
                            break;
                        case Key.Down:
                            if (CanMoveElements(VM.SelectedElements, e.Key))
                            {
                                VM.SelectedElements.ForEach(x => x.Height++);
                            }
                            break;
                        case Key.Left:
                            VM.SelectedElements.ForEach(x => { if (x.Width > 1) x.Width--; });
                            break;
                        case Key.Up:
                            VM.SelectedElements.ForEach(x => { if (x.Height > 1) x.Height--; });
                            break;
                    }
                    VM.UndoRedo.NewUserAction(VM.ElementsOnWorkSpace);
                }
                else
                {
                    // Commands
                    switch (e.Key)
                    {
                        case Key.C:
                            command = VM.Copy;
                            break;
                        case Key.V:
                            command = VM.Paste;
                            break;
                        case Key.Z:
                            command = VM.Undo;
                            break;
                        case Key.Y:
                            command = VM.Redo;
                            break;
                        case Key.A:
                            command = VM.SelectAll;
                            break;
                        case Key.X:
                            command = VM.Cut;
                            break;
                    }
                }
            }
            else if (e.Key == Key.Delete)
            {
                command = VM.Delete;
            }
            else if (VM.SelectedElements.Count > 0 && (e.Key == Key.Right || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Up))
            {
                // Element moving

                if (!CanMoveElements(VM.SelectedElements, e.Key))
                {
                    return;
                }

                switch (e.Key)
                {
                    case Key.Right:
                        VM.SelectedElements.ForEach(x => x.CoordX++);
                        break;
                    case Key.Down:
                        VM.SelectedElements.ForEach(x => x.CoordY++);
                        break;
                    case Key.Left:
                        VM.SelectedElements.ForEach(x => x.CoordX--);
                        break;
                    case Key.Up:
                        VM.SelectedElements.ForEach(x => x.CoordY--);
                        break;
                }

                // Update canvas position
                VM.SelectedElements.ForEach(x => { Canvas.SetLeft(x, x.CoordX); Canvas.SetTop(x, x.CoordY); });
                VM.UndoRedo.NewUserAction(VM.ElementsOnWorkSpace);
            }

            if (command != null && command.CanExecute(null))
            {
                command.Execute(null);
            }
        }

        bool CanMoveElements(List<ScreenElement> elementsToMove, Key direction)
        {
            bool canMove = true;
            foreach (var element in elementsToMove)
            {
                switch (direction)
                {
                    case Key.Right:
                        if (element.CoordX + element.ActualWidth >= WorkSpace.ActualWidth)
                        {
                            canMove = false;
                        }
                        break;
                    case Key.Down:
                        if (element.CoordY + element.ActualHeight >= WorkSpace.ActualHeight)
                        {
                            canMove = false;
                        }
                        break;
                    case Key.Left:
                        if (element.CoordX <= 0)
                        {
                            canMove = false;
                        }
                        break;
                    case Key.Up:
                        if (element.CoordY <= 0)
                        {
                            canMove = false;
                        }
                        break;
                }
            }

            return canMove;
                
        }

        void CreateTmpElementsWithOpacity(List<ScreenElement> copyFromElements)
        {
            for (int i = 0; i < copyFromElements.Count; i++)  
            {
                var type = copyFromElements[i].GetType();
                var tmpElement = (ScreenElement)Activator.CreateInstance(type);
                tmpElement.Id = -9999 + i;
                tmpElement.Name = $"TMP_FOLLOWER_{i}";
                tmpElement.Opacity = 0.5;
                tmpElement.CoordX = copyFromElements[i].CoordX;
                tmpElement.CoordY = copyFromElements[i].CoordY;

                tmpElement.MouseLeftButtonUp += Element_MouseLeftButtonUp;

                TmpFollowerElements.Add(tmpElement);

                WorkSpace.Children.Add(tmpElement);
                Canvas.SetLeft(tmpElement, tmpElement.CoordX);
                Canvas.SetTop(tmpElement, tmpElement.CoordY);
            }
        }

        void RemoveTmpElementsWithOpacity()
        {
            TmpFollowerElements.ForEach(x => WorkSpace.Children.Remove(x));
            TmpFollowerElements.Clear();
        }


        void ChangeCoorditanesOnMoving(List<ScreenElement> movingElements,
            Point currentPosition,
            int selectedElementIndexByMouse,
            double coordPressedOnElementX, double coordPressedOnElementY)
        {
            double newPositionX = currentPosition.X - coordPressedOnElementX;
            double newPositionY = currentPosition.Y - coordPressedOnElementY;

            if (selectedElementIndexByMouse < 0)
            {
                return;
            }

            double offsetX = newPositionX - movingElements[selectedElementIndexByMouse].CoordX;
            double offsetY = newPositionY - movingElements[selectedElementIndexByMouse].CoordY;

            // calculate offset for pressed element for new coordinates
            // use this offset for each selected element - calculate new coordinates
            // set new coordinates for each element

            // check for each element the border of workspace. If border reached - break
            // we can move one element a little off the board, but if there is a group - nonono
            if (movingElements.Count == 1)
            {
                if (currentPosition.X >= WorkSpace.ActualWidth || currentPosition.X <= 0
                   || currentPosition.Y >= WorkSpace.ActualHeight || currentPosition.Y <= 0)
                {
                    return;
                }
            }
            else
            {
                foreach (var element in movingElements)
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
            foreach (var element in movingElements)
            {
                element.CoordX += offsetX;
                element.CoordY += offsetY;
                Canvas.SetLeft(element, element.CoordX);
                Canvas.SetTop(element, element.CoordY);
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;

            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta);
                e.Handled = true;
            }
        }
    }




}
