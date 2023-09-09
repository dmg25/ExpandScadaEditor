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
using ExpandScadaEditor.ScreenEditor.Items;

namespace ExpandScadaEditor.ScreenEditor
{
    /// <summary>
    /// Логика взаимодействия для WorkspaceCanvas.xaml
    /// </summary>
    public partial class WorkspaceCanvas : Canvas
    {
        const string SELECTING_RECTANGLE = "SELECTING_RECTANGLE";


        List<(int id, double xCoord, double yCoord)> selectedPositionsBeforeMoving = new List<(int id, double xCoord, double yCoord)>();
        MouseMovingMode preMode = MouseMovingMode.None;
        ScreenElement tmpPreSelectedElement = new ScreenElement();
        int selectedElementIndexByMouse = -1;
        internal List<ScreenElement> TmpFollowerElements = new List<ScreenElement>();


        double SelectedElementMousePressedCoordX { get; set; }
        double SelectedElementMousePressedCoordY { get; set; }
        bool elementsWereMoved = false;

        ElementsSelectingBorder borderSelecting;

        ScreenElement elementFromCatalog;
        //ScreenElement tmpElementFromCatalog;
        bool elementFromCatalogMoved = false;





        // Parental variables - must be initialized with editor
        internal VectorEditorVM VM;
        internal ScrollViewer ParentalScroller; 



        private double zoomCoef = 1;
        public double ZoomCoef
        {
            get
            {
                return zoomCoef;
            }
            set
            {
                if (value <= 0)
                {
                    return;
                }

                //if (zoomCoef != value)
                //{
                //ChangeZoom(value);
                //}
                zoomCoef = value;
                //NotifyPropertyChanged();
            }
        }

        private double width;
        public new double Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
                base.Width = Width * ZoomCoef;
                //NotifyPropertyChanged();
            }
        }

        private double height;
        public new double Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
                base.Height = Height * ZoomCoef;
                //NotifyPropertyChanged();
            }
        }


        public WorkspaceCanvas()
        {
            InitializeComponent();

            this.MouseMove += WorkSpace_MouseMove;

        }

        internal void Initialize(VectorEditorVM vm, ScrollViewer parentalScrollViewer)
        {
            VM = vm;
            ParentalScroller = parentalScrollViewer;

            VM.NewScreenElementAdded += VM_NewScreenElementAdded;
            VM.ScreenElementReplaced += VM_ScreenElementReplaced;
            VM.ScreenElementDeleted += VM_ScreenElementDeleted;
            VM.SelectedElementsDeleted += VM_SelectedElementsDeleted;
            VM.SelectTheseElements += VM_SelectTheseElements;
            VM.ZoomChanged += VM_ZoomChanged;

            MouseLeftButtonDown += WorkSpace_MouseLeftButtonDown;
            MouseLeftButtonUp += WorkSpace_MouseLeftButtonUp;
        }

        private void VM_ZoomChanged(object sender, EventArgs e)
        {

        }


        // TODO move to screenElement
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

        private void VM_ScreenElementDeleted(object sender, ScreenElementEventArgs e)
        {
            e.Element.MouseLeftButtonDown -= Element_MouseLeftButtonDown;
            e.Element.MouseLeftButtonUp -= Element_MouseLeftButtonUp;

            e.Element.OnElementResizing -= Element_OnElementResizing;
            e.Element.ElementSizeChanged -= Element_ElementResized;

            //WorkSpace.Children.Remove(WorkSpace.Children.fir);

            this.Children.Remove(e.Element);
            e.Element = null;
        }


        private void VM_ScreenElementReplaced(object sender, ReplacingElementEventArgs e)
        {
            if (e.OldElement != null)
            {
                e.OldElement.MouseLeftButtonDown -= Element_MouseLeftButtonDown;
                e.OldElement.MouseLeftButtonUp -= Element_MouseLeftButtonUp;
                e.OldElement.OnElementResizing -= Element_OnElementResizing;
                e.OldElement.ElementSizeChanged -= Element_ElementResized;
                this.Children.Remove(e.OldElement);
                e.OldElement = null;
            }
            VM_NewScreenElementAdded(null, new ScreenElementEventArgs() { Element = e.NewElement });
        }

        private void VM_NewScreenElementAdded(object sender, ScreenElementEventArgs e)
        {
            this.Children.Add(e.Element);
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
            /*  Zoom and resizing 
             *      - send every time actual width/height to elements on canvas (just in properties)
             *          - send them on creation too (maybe create special methid aka "actualize to current world")
             *          - create also new ersatz properties and use them
             *          - do the same for zoom coef
             *          - BUT NOT HERE - in VM
             *      - ??? what do we do if we changed size of workspace, but there are some elements close to border?
             *          - probably nothing - let them be out of range, just be sure - no crush
             *          - and we have to be able to see tree of elements - find them and change position
             *      - Move as must as possible action to element itself. e.g. 100% resiting functions
             *          - moving make here as short as possible
             *          
             *      
             * 
             * 
             * 
             * 
             * 
             * 
             * */







            var element = sender as ScreenElement;
            switch (e.ResizingType)
            {
                case ResizingType.ChangeSize:
                    if (!double.IsNaN(e.NewWidth) && e.NewWidth + element.CoordX < this.ActualWidth)
                    {
                        element.Width = e.NewWidth;
                    }
                    if (!double.IsNaN(e.NewHeight) && e.NewHeight + element.CoordY < this.ActualHeight)
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
                this.Children.Remove(borderSelecting);
                borderSelecting = null;
            }
            this.ReleaseMouseCapture();
        }

        private void WorkSpace_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Here is a logic for Canvas event only. If we clicked on any element - igrore this event
            if (e.OriginalSource is not Canvas)
            {
                return;
            }

            // start of selecting by mouse
            var mousePosition = e.GetPosition(this);

            // drop all selected elements if there were before
            DeselectAllElements();

            borderSelecting = new ElementsSelectingBorder();
            borderSelecting.AddBorderOnWorkspace(SELECTING_RECTANGLE, this, mousePosition);
            this.CaptureMouse();

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
            selectedPositionsBeforeMoving.Clear();
        }

        //private void ElementCatalog_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        //{
        //    if (elementFromCatalog is not null)
        //    {
        //        if (elementFromCatalogMoved)
        //        {
        //            elementFromCatalogMoved = false;
        //            if (TmpFollowerElements[0].CoordX > 0 && TmpFollowerElements[0].CoordX < WorkSpace.ActualWidth
        //            && TmpFollowerElements[0].CoordY > 0 && TmpFollowerElements[0].CoordY < WorkSpace.ActualHeight)
        //            {
        //                elementFromCatalog.CoordX = TmpFollowerElements[0].CoordX;
        //                elementFromCatalog.CoordY = TmpFollowerElements[0].CoordY;
        //                RemoveTmpElementsWithOpacity();
        //            }
        //            else
        //            {
        //                RemoveTmpElementsWithOpacity();
        //                elementFromCatalog = null;
        //                Cursor = Cursors.Arrow;
        //                return;
        //            }
        //        }
        //        else
        //        {
        //            var position = FindPlaceForNewElement(elementFromCatalog);
        //            elementFromCatalog.CoordX = position.x;
        //            elementFromCatalog.CoordY = position.y;
        //        }

        //        VM.AddNewScreenElement(elementFromCatalog);
        //        VM.UndoRedo.NewUserAction(VM.ElementsOnWorkSpace, UndoRedoActionType.Create);
        //    }

        //    Cursor = Cursors.Arrow;
        //}



        //private void ElementCatalog_MouseMove(object sender, MouseEventArgs e)
        //{
        //    if (e.LeftButton == MouseButtonState.Pressed && elementFromCatalog is not null)
        //    {
        //        //TranslateTransform transform = new TranslateTransform();
        //        //transform.X = Mouse.GetPosition(WorkSpace).X;
        //        //transform.Y = Mouse.GetPosition(WorkSpace).Y;

        //        // !!! ATTENTION !!! we decide that here elementFromCatalog can be only one, so we will use only first element in follower tmp list

        //        Point point = new Point(Mouse.GetPosition(WorkSpace).X, Mouse.GetPosition(WorkSpace).Y);

        //        if (!elementFromCatalogMoved)
        //        {
        //            elementFromCatalogMoved = true;
        //            // create new element on the CANVAS layer and set opacity 0,5 and -1 ID
        //            // we can set coordinates to catalog's element, because we've copied it
        //            elementFromCatalog.CoordX = point.X;
        //            elementFromCatalog.CoordY = point.Y;
        //            CreateTmpElementsWithOpacity(new List<ScreenElement>() { elementFromCatalog });

        //            ElementCatalog.Cursor = Cursors.SizeAll;
        //            Cursor = Cursors.SizeAll;
        //        }
        //        else
        //        {
        //            TmpFollowerElements[0].CoordX = point.X;
        //            TmpFollowerElements[0].CoordY = point.Y;
        //            Canvas.SetLeft(TmpFollowerElements[0], TmpFollowerElements[0].CoordX);
        //            Canvas.SetTop(TmpFollowerElements[0], TmpFollowerElements[0].CoordY);
        //        }
        //    }
        //}

        private void WorkSpace_MouseMove(object sender, MouseEventArgs e)
        {
            //if (e.Source != WorkSpace)
            //{
            //    ScrollOnMoving(e.GetPosition(WorkSpace));
            //}

            //--- test / delete later ---
            var mousePosition = e.GetPosition(this);
            VM.MouseX = mousePosition.X;
            VM.MouseY = mousePosition.Y;
            //---------------------------
            MouseMovingMode currentMouseMovingMode = MouseMovingMode.None;

            if (e.LeftButton == MouseButtonState.Pressed && borderSelecting != null)
            {
                currentMouseMovingMode = MouseMovingMode.Selecting;
            }
            else if (e.LeftButton == MouseButtonState.Pressed && VM.SelectedElements.Count != 0)
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
            //VM.MouseOverElement = $"+++++++++++++++++++++{DateTime.Now.ToString("fff")}";
            switch (currentMovingMode)
            {
                case MouseMovingMode.MoveSelectedElements:
                    //ScrollOnMoving(currentPosition);
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
                    if (!elementsWereMoved)
                    {
                        SelectedElementsToMovingMode();
                        elementsWereMoved = true;
                    }

                    //ScrollOnMoving(currentPosition);

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
                        ScrollOnMoving(currentPosition);

                        // redraw border
                        borderSelecting.ContinueSelection(currentPosition);

                        // selecting 
                        ActualizeElementsCoveredBySelection();

                        //VM.MouseOverElement = $"____________________________________________________________________________-{DateTime.Now.ToString("fff")}";
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


        internal (double x, double y) FindPlaceForNewElement(ScreenElement element)
        {
            double centerX = this.ActualWidth / 2;
            double centerY = this.ActualHeight / 2;

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

                    if (newCenterX + element.Width >= this.ActualWidth)
                    {
                        newCenterX = this.ActualWidth - element.Width;
                        endX = true;
                    }

                    if (newCenterY + element.Height >= this.ActualHeight)
                    {
                        newCenterY = this.ActualHeight - element.Height;
                        endY = true;
                    }

                    centerX = newCenterX < 0 ? 0 : newCenterX;
                    centerY = newCenterY < 0 ? 0 : newCenterY;
                }
            }

            return (centerX, centerY);

        }

        internal void ChangeCoorditanesOnMoving(List<ScreenElement> movingElements,
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
                if (currentPosition.X >= this.ActualWidth || currentPosition.X <= 0
                   || currentPosition.Y >= this.ActualHeight || currentPosition.Y <= 0)
                {
                    return;
                }
            }
            else
            {
                foreach (var element in movingElements)
                {
                    if (element.CoordX + offsetX + element.ActualWidth >= this.ActualWidth
                        || element.CoordY + offsetY + element.ActualHeight >= this.ActualHeight
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


        internal void CreateTmpElementsWithOpacity(List<ScreenElement> copyFromElements)
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

                this.Children.Add(tmpElement);
                Canvas.SetLeft(tmpElement, tmpElement.CoordX);
                Canvas.SetTop(tmpElement, tmpElement.CoordY);
            }
        }



        internal void RemoveTmpElementsWithOpacity()
        {
            TmpFollowerElements.ForEach(x => this.Children.Remove(x));
            TmpFollowerElements.Clear();
        }



        void ScrollOnMoving(Point currentMousePosition)
        {
            /*  Call this method in the end of every moving cycle
             *      - creating new item from catalog
             *      - copy & move
             *      - selecting
             *      - moving
             *      - resizing
             *  check if mouse pointer is out of shown workspace area
             *      - if workspace can be moved - move it on 5 px or less if there is no 5 px left
             *      - check if holding position can continue moving. If not 
             *          - try recurse 
             *      - check if every item is still moving too
             * 
             * 
             * */

            // TODO complete it with backmoving
            if (currentMousePosition.X >= ParentalScroller.ViewportWidth - ParentalScroller.HorizontalOffset - 10)
            {
                ParentalScroller.ScrollToHorizontalOffset(ParentalScroller.HorizontalOffset + 0.5);
            }

            if (currentMousePosition.Y >= ParentalScroller.ViewportHeight - ParentalScroller.VerticalOffset - 10)
            {
                ParentalScroller.ScrollToVerticalOffset(ParentalScroller.VerticalOffset + 0.5);
            }

            // just tests
            //var fff = this.ParentalScroller;
            //var viewportH = WSScroller.ViewportHeight;
            //var viewportW = WSScroller.ViewportWidth;
            //var verticalOffset = WSScroller.VerticalOffset; // from left
            //var horizontalOffset = WSScroller.HorizontalOffset; // from top
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

        void SelectElement(ScreenElement element)
        {
            // add to the dictionaly and add border around
            VM.SelectedElements.Add(element);
            element.ShowResizeBorder();
        }







    }
}
