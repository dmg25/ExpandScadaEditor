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
     *   +++MOVE WORKSPACE WITH SCROLL OPERATIONS BY MOUSE + KEYS
     *      
     *   +++SHOW EMPTYNESS IF WINDOW IS BIGGER THEN WORKSPACE
     *      
     *   +++MOVING OPERATIONS ONLY INSIDE OF THE REAL WORKSPACE
     *      
     *   !!!SCROLL WORKSPACE DURING THE MOVING
     *          - For selecting works perfect: if we on the side of workspace - move event call itself automatically and we move
     *          - For moving element and resizing doesn't work - we have to move a mouse all the time if we want to move whole workspace
     *              - This is because of some event rising tricks. But this is not clear at all.. Have to make a lot of tests to find solution...
     *          PROBLEM
     *              - On element moving event of moving works not so good because of mouse capture state focused on element automatically
     *              - If we write WorkSpace.CaptureMouse() on mouse down event - moving works perfect
     *              - !!! BUT !!! on mouse Up event we will see source as canvas EVERY TIME. So this is a problem we have to solve carefully
     *      
     *      
     *   +++CREATE ZOOM FUNCTIONS: TOOLS/MOUSE WHEEL + CTRL...
     *          - Create  + and - commands for zooming
     *          - each step of this command must change zoom lets say on 10%
     *          - For rendering use original element's settings, but user has to see properties for him only (without scaling)
     *            Create double properties
     *          - Changing properties
     *              - Width/Height
     *              - X/Y coordinates
     *              - Text font
     *              - Line thickness? 
     *              - Resize borders (???)
     *          - Add scrollbar to the bottom for resizing
     *      
     *    ###Show settings
     *          - of selected elements and workspace if nothing selected
     *          - do it as a list on a right side
     *          - with validation
     *          - with default values
     *          - with different types:
     *              - textbox
     *              - combobox
     *              - color selection
     *              - ...
     *          - 
     *      
     *      
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


    //TODO minimum list BEFORE go to next step
    // *
    // *  1. Catalog list does not show last element - fix it
    // *  2. Plan and create saving method.
    // *      - Add button for saving with file type selecton
    // *      - Find out how to save all elements without borders and root grid if not necessary
    // *      - save only properties from list.Others - dont care
    // *      - manually add some bindings to existing signals
    // *          - Coordinates
    // *          - size
    // *          - opacity
    // *  3. Start and test that saved screens can work
    // *



    /*  TODO FUNCTIONALITY
     *  
     *  1. When a group of elements selected show special icon "+" with arrows, to move whole group. 
     *     There could be situation, that selected elements are too small to move them by pressing on them
     * 
     *  2. Add rotation if one element is selected - show icon for rotating and catch mause over it
     *  
     *  3. Show scrollbar with zoom below
     *  
     *  4. Show coordinates on bottom and delete test coordinates
     *  
     *  5. Check copying and adding of new element with dragging. There is a lot of crashes and moving is not finished sometimes. 
     *     Sometimes undo/redo throw an exception after moving/copying finished
     *     
     *  6. Add rotation of element as commands +/- 90 deg
     *  
     *  7. Do not show red border over selected elements, only if it is not selected
     *  
     *  8. Do not show gray border above resizing rectangles
     *  
     *  9. Add scroll on element moving - it is not really simple
     *  
     *  10. If at least one element selected and user press arrows on keyboard - do not move workspace with element. Only element
     *  
     *  11. Complete zooming functions:
     *      - border thickness
     *      - fext font
     *      - calculate min/max limits for this. If we can not show text bigger than that or smaller - do not allow do it
     *      
     *  12. 
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
        //const string SELECTING_RECTANGLE = "SELECTING_RECTANGLE";
        //const string SELECTED_RECTANGLE = "SELECTED_RECTANGLE";

        //MouseMovingMode CurrentMouseMovingMode = MouseMovingMode.None;

        protected VectorEditorVM VM
        {
            get { return (VectorEditorVM)Resources["ViewModel"]; }
        }

        ScreenElement elementFromCatalog;
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

            
            //VM.Initialize();

            //// Here we have to save original state of this workspace. Opened new or loaded - here must be first point
            //VM.UndoRedo.BasicUserAction(VM.ElementsOnWorkSpace.Values.ToList());

            //// events for creating element 
            //ElementCatalog.MouseLeftButtonUp += ElementCatalog_MouseLeftButtonUp;
            //ElementCatalog.MouseMove += ElementCatalog_MouseMove;
        }

        private void ElementCatalog_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && elementFromCatalog is not null)
            {
                //TranslateTransform transform = new TranslateTransform();
                //transform.X = Mouse.GetPosition(WorkSpace).X;
                //transform.Y = Mouse.GetPosition(WorkSpace).Y;

                // !!! ATTENTION !!! we decide that here elementFromCatalog can be only one, so we will use only first element in follower tmp list

                var point = WorkspaceCanvas.UnzoomCoordinates(Mouse.GetPosition(WorkSpace), WorkSpace.ZoomCoef) ;

                //Point point = new Point(Mouse.GetPosition(WorkSpace).X, Mouse.GetPosition(WorkSpace).Y);

                if (!elementFromCatalogMoved)
                {
                    elementFromCatalogMoved = true;
                    // create new element on the CANVAS layer and set opacity 0,5 and -1 ID
                    // we can set coordinates to catalog's element, because we've copied it
                    elementFromCatalog.CoordX = point.X;
                    elementFromCatalog.CoordY = point.Y;
                    WorkSpace.CreateTmpElementsWithOpacity(new ObservableCollection<ScreenElement>() { elementFromCatalog });

                    ElementCatalog.Cursor = Cursors.SizeAll;
                    Cursor = Cursors.SizeAll;
                }
                else
                {
                    WorkSpace.TmpFollowerElements[0].CoordX = point.X;
                    WorkSpace.TmpFollowerElements[0].CoordY = point.Y;
                    Canvas.SetLeft(WorkSpace.TmpFollowerElements[0], WorkSpace.TmpFollowerElements[0].ZoomedCoordX);
                    Canvas.SetTop(WorkSpace.TmpFollowerElements[0], WorkSpace.TmpFollowerElements[0].ZoomedCoordY);
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
                    if (WorkSpace.TmpFollowerElements[0].CoordX > 0 && WorkSpace.TmpFollowerElements[0].CoordX < WorkSpace.Width
                    && WorkSpace.TmpFollowerElements[0].CoordY > 0 && WorkSpace.TmpFollowerElements[0].CoordY < WorkSpace.Height)
                    {
                        elementFromCatalog.CoordX = WorkSpace.TmpFollowerElements[0].CoordX;
                        elementFromCatalog.CoordY = WorkSpace.TmpFollowerElements[0].CoordY;
                        WorkSpace.RemoveTmpElementsWithOpacity();
                    }
                    else
                    {
                        WorkSpace.RemoveTmpElementsWithOpacity();
                        elementFromCatalog = null;
                        Cursor = Cursors.Arrow;
                        return;
                    }
                }
                else
                {
                    var position = WorkSpace.FindPlaceForNewElement(elementFromCatalog);
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
                elementFromCatalog = (ScreenElement)Activator.CreateInstance(element.GetType(), element.ElementContent);
                elementFromCatalog.InitializeFromAnotherElement(element);
                //element.CatalogMode = true;
            }
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

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();

            // TODO when you add properties for workspace size and it will be initialized in VM - move it there!
            

            WorkSpace.Initialize(VM, WSScroller);
            VM.Initialize();

            WorkSpace.Width = 500;
            WorkSpace.Height = 500;
            WorkSpace.Background = Brushes.LightGray;
            VM.WorkSpaceHeight = WorkSpace.Height;//WorkSpace.ActualHeight;
            VM.WorkSpaceWidth = WorkSpace.Width;//WorkSpace.ActualWidth;

            // Here we have to save original state of this workspace. Opened new or loaded - here must be first point
            VM.UndoRedo.BasicUserAction(VM.ElementsOnWorkSpace.Values.ToList());

            // events for creating element 
            ElementCatalog.MouseLeftButtonUp += ElementCatalog_MouseLeftButtonUp;
            ElementCatalog.MouseMove += ElementCatalog_MouseMove;
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
                            if (WorkSpace.CanMoveElements(VM.SelectedElements, e.Key))
                            {
                                //VM.SelectedElements.ForEach(x => x.Width++);
                                foreach (var item in VM.SelectedElements)
                                {
                                    item.Width++;
                                }
                            }
                            break;
                        case Key.Down:
                            if (WorkSpace.CanMoveElements(VM.SelectedElements, e.Key))
                            {
                                //VM.SelectedElements.ForEach(x => x.Height++);
                                foreach (var item in VM.SelectedElements)
                                {
                                    item.Height++;
                                }
                            }
                            break;
                        case Key.Left:
                            //VM.SelectedElements.ForEach(x => { if (x.Width > 1) x.Width--; });
                            foreach (var item in VM.SelectedElements)
                            {
                                if (item.Width > 1) item.Width--;
                            }
                            break;
                        case Key.Up:
                            //VM.SelectedElements.ForEach(x => { if (x.Height > 1) x.Height--; });
                            foreach (var item in VM.SelectedElements)
                            {
                                if (item.Height > 1) item.Height--;
                            }
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

                if (!WorkSpace.CanMoveElements(VM.SelectedElements, e.Key))
                {
                    return;
                }

                switch (e.Key)
                {
                    case Key.Right:
                        //VM.SelectedElements.ForEach(x => x.CoordX++);
                        foreach (var item in VM.SelectedElements)
                        {
                            item.CoordX++;
                        }
                        break;
                    case Key.Down:
                        //VM.SelectedElements.ForEach(x => x.CoordY++);
                        foreach (var item in VM.SelectedElements)
                        {
                            item.CoordY++;
                        }
                        break;
                    case Key.Left:
                        //VM.SelectedElements.ForEach(x => x.CoordX--);
                        foreach (var item in VM.SelectedElements)
                        {
                            item.CoordX--;
                        }
                        break;
                    case Key.Up:
                        //VM.SelectedElements.ForEach(x => x.CoordY--);
                        foreach (var item in VM.SelectedElements)
                        {
                            item.CoordY--;
                        }
                        break;
                }

                // Update canvas position
                //VM.SelectedElements.ForEach(x => { Canvas.SetLeft(x, x.ZoomedCoordX); Canvas.SetTop(x, x.ZoomedCoordY); });
                foreach (var item in VM.SelectedElements)
                {
                    Canvas.SetLeft(item, item.ZoomedCoordX); Canvas.SetTop(item, item.ZoomedCoordY);
                }
                VM.UndoRedo.NewUserAction(VM.ElementsOnWorkSpace);
            }

            if (command != null && command.CanExecute(null))
            {
                command.Execute(null);
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
            else if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                ICommand command = null;

                if (e.Delta > 0)
                {
                    command = VM.ZoomIn;
                }
                else if (e.Delta < 0)
                {
                    command = VM.ZoomOut;
                }

                if (command != null && command.CanExecute(null))
                {
                    command.Execute(null);
                }

                e.Handled = true;
            }
        }
    }




}
