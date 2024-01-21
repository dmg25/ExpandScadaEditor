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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ExpandScadaEditor.ScreenEditor.WorkspaceHelperControls;
using ExpandScadaEditor.ScreenEditor.Items;
using System.Collections.ObjectModel;
using System.Reflection;
using ExpandScadaEditor.ScreenEditor.Items.Properties;

namespace ExpandScadaEditor.ScreenEditor
{
    /// <summary>
    /// Логика взаимодействия для WorkspaceCanvas.xaml
    /// </summary>
    public partial class WorkspaceCanvas : Canvas, INotifyPropertyChanged
    {
        const string SELECTING_RECTANGLE = "SELECTING_RECTANGLE";


        List<(int id, double xCoord, double yCoord)> selectedPositionsBeforeMoving = new List<(int id, double xCoord, double yCoord)>();
        MouseMovingMode preMode = MouseMovingMode.None;
        ScreenElement tmpPreSelectedElement = new ScreenElement(new ScreenElementContent("unknown")); // TODO ??? was drunk ???
        int selectedElementIndexByMouse = -1;
        internal ObservableCollection<ScreenElement> TmpFollowerElements = new ObservableCollection<ScreenElement>();


        double SelectedElementMousePressedCoordX { get; set; }
        double SelectedElementMousePressedCoordY { get; set; }
        bool elementsWereMoved = false;

        ElementsSelectingBorder borderSelecting;

        ScreenElement elementFromCatalog;
        //ScreenElement tmpElementFromCatalog;
        bool elementFromCatalogMoved = false;
        Point tmpMouseCoordinates = new Point();


        private ObservableCollection<GroupOfProperties> elementPropertyGroups = new ObservableCollection<GroupOfProperties>();
        public ObservableCollection<GroupOfProperties> ElementPropertyGroups
        {
            get
            {
                return elementPropertyGroups;
            }
            set
            {
                elementPropertyGroups = value;
            }
        }


        // Parental variables - must be initialized with editor
        internal VectorEditorVM VM;
        internal ScrollViewer ParentalScroller;

        //Refactoring:
        // Move as much as possible to screen element class
        // check if working
        // fix all zoom conflicts..

        /*  Zooming rules
         *      - if we move element
         *          - add new parameters "zoomed coordinates"
         *          - every time when we want to update coordinates by moving and use mouse coordinates - update new params and calc real coords unzoomed
         *          - show mouse coordinates unzoomed too!
         *      - if we resize element
         *          - check every place where we can change size of element
         *          - when we do it - update local size with dividing by zoom coef
         *          - everywhere where we check actual size of element - check unzoomed settings
         *      - if we create element
         *      - if we copy/paste elements
         *      - if we select elements by selecting border
         * 
         * 
         * */

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

                base.Width = Width * zoomCoef;
                base.Height = Height * zoomCoef;

                foreach (var pairs in VM.ElementsOnWorkSpace)
                {
                    pairs.Value.ZoomCoef = value;
                }

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
                
                foreach (var pairs in VM.ElementsOnWorkSpace)
                {
                    pairs.Value.WorkspaceWidth = value;
                }


                OnPropertyChanged();
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
                foreach (var pairs in VM.ElementsOnWorkSpace)
                {
                    pairs.Value.WorkspaceHeight = value;
                }
                OnPropertyChanged();
            }
        }


        public WorkspaceCanvas()
        {
            InitializeComponent();

            this.MouseMove += WorkSpace_MouseMove;

            GroupOfProperties newGroup = new GroupOfProperties("Common", false, new ObservableCollection<ElementProperty>()
            {
                CreateEditableProperty<double>(nameof(Height), "Height in px", ScreenElement.PositiveDoubleValidation),
                CreateEditableProperty<double>(nameof(Width), "Width in px" , ScreenElement.PositiveDoubleValidation),
            });

            ElementPropertyGroups.Add(newGroup);

        }

        internal void Initialize(VectorEditorVM vm, ScrollViewer parentalScrollViewer)
        {
            //GroupOfProperties newGroup = new GroupOfProperties("Common", false, new ObservableCollection<ElementProperty>()
            //{
            //    CreateEditableProperty<double>(nameof(Height), "Height in px", ScreenElement.PositiveDoubleValidation),
            //    CreateEditableProperty<double>(nameof(Width), "Width in px" , ScreenElement.PositiveDoubleValidation),
            //});

            //ElementPropertyGroups.Add(newGroup);



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

            // TODO move it somwhere else...
            foreach (var pairs in VM.ElementsOnWorkSpace)
            {
                pairs.Value.Width = Width;
                pairs.Value.Height = Height;
            }
        }

        internal void Deinitialize()
        {
            

            VM.NewScreenElementAdded -= VM_NewScreenElementAdded;
            VM.ScreenElementReplaced -= VM_ScreenElementReplaced;
            VM.ScreenElementDeleted -= VM_ScreenElementDeleted;
            VM.SelectedElementsDeleted -= VM_SelectedElementsDeleted;
            VM.SelectTheseElements -= VM_SelectTheseElements;
            VM.ZoomChanged -= VM_ZoomChanged;

            VM = null;
            ParentalScroller = null;

            MouseLeftButtonDown -= WorkSpace_MouseLeftButtonDown;
            MouseLeftButtonUp -= WorkSpace_MouseLeftButtonUp;
        }

        private void VM_ZoomChanged(object sender, EventArgs e)
        {
            ZoomCoef = VM.ZoomCoef;
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

        private void VM_ScreenElementDeleted(object sender, ScreenElementEventArgs e)
        {
            e.Element.MouseLeftButtonDown -= Element_MouseLeftButtonDown;
            e.Element.MouseLeftButtonUp -= Element_MouseLeftButtonUp;

            //e.Element.OnElementResizing -= Element_OnElementResizing;
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
                //e.OldElement.OnElementResizing -= Element_OnElementResizing;
                e.OldElement.ElementSizeChanged -= Element_ElementResized;
                this.Children.Remove(e.OldElement);
                e.OldElement = null;
            }
            VM_NewScreenElementAdded(null, new ScreenElementEventArgs() { Element = e.NewElement });
        }

        private void VM_NewScreenElementAdded(object sender, ScreenElementEventArgs e)
        {
            this.Children.Add(e.Element);
            Canvas.SetLeft(e.Element, e.Element.ZoomedCoordX);
            Canvas.SetTop(e.Element, e.Element.ZoomedCoordY);
            e.Element.MouseLeftButtonDown += Element_MouseLeftButtonDown;
            e.Element.MouseLeftButtonUp += Element_MouseLeftButtonUp;
            //e.Element.OnElementResizing += Element_OnElementResizing;
            e.Element.ElementSizeChanged += Element_ElementResized;
            e.Element.WorkspaceHeight = this.Height;
            e.Element.WorkspaceWidth = this.Width;
        }




        private void Element_ElementResized(object sender, EventArgs e)
        {
            VM.UndoRedo.NewUserAction(VM.ElementsOnWorkSpace);
            // VM.UndoRedo.NewUserAction(sender as ScreenElement);
        }


        private void WorkSpace_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // end of selecting by mouse
            if (borderSelecting != null)
            {
                this.Children.Remove(borderSelecting);
                borderSelecting = null;
            }

            // Check if there was no moving - show cnavas properties
            var mousePosition = WorkspaceCanvas.UnzoomCoordinates(e.GetPosition(this), ZoomCoef);

            VM.IsWorkspaceSelected = VM.SelectedElements.Count == 0 && mousePosition.X == tmpMouseCoordinates.X && mousePosition.Y == tmpMouseCoordinates.Y;

            this.ReleaseMouseCapture();
        }

        private void WorkSpace_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Here is a logic for Canvas event only. If we clicked on any element - igrore this event
            if (e.OriginalSource is not Canvas)
            {
                VM.IsWorkspaceSelected = false;
                return;
            }

            // start of selecting by mouse
            var mousePosition = WorkspaceCanvas.UnzoomCoordinates(e.GetPosition(this), ZoomCoef);

            // save coordinates to check if position was changed after mouse up
            tmpMouseCoordinates = mousePosition;

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

            //if (VM.SelectedElements.Find(x => x.Id == element.Id) is null)
            if (VM.SelectedElements.FirstOrDefault(x => x.Id == element.Id) is null)
            {
                if (!Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    DeselectAllElements();
                }

                SelectElement(element);
                tmpPreSelectedElement = element;
            }

            //SelectedElement = sender as ScreenElement;
            var mousePosition = WorkspaceCanvas.UnzoomCoordinates(e.GetPosition(element), ZoomCoef);
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
                        //if (VM.SelectedElements.Find(x => x.Id == element.Id) is null)
                        if (VM.SelectedElements.FirstOrDefault(x => x.Id == element.Id) is null)
                        {
                            SelectElement(element);
                        }
                    }
                }
                else
                {
                    DeselectAllElements();
                    //if (VM.SelectedElements.Find(x => x.Id == element.Id) is null)
                    if (VM.SelectedElements.FirstOrDefault(x => x.Id == element.Id) is null)
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
                    VM_SelectTheseElements(null, new ScreenElementsEventArgs() { Elements = newElementsList.ToList() });
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


        private void WorkSpace_MouseMove(object sender, MouseEventArgs e)
        {
            //if (e.Source != WorkSpace)
            //{
            //    ScrollOnMoving(e.GetPosition(WorkSpace));
            //}

            //--- test / delete later ---
            var mousePosition = WorkspaceCanvas.UnzoomCoordinates(e.GetPosition(this), ZoomCoef);
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
                //VM.SelectedElements.ForEach(x => selectedPositionsBeforeMoving.Add((x.Id, x.CoordX, x.CoordY)));
                foreach (var item in VM.SelectedElements)
                {
                    selectedPositionsBeforeMoving.Add((item.Id, item.CoordX, item.CoordY));
                }
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
                            Canvas.SetLeft(element, element.ZoomedCoordX);
                            Canvas.SetTop(element, element.ZoomedCoordY);
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
                        Point elementRect2 = new Point(element.Value.CoordX + element.Value.Width, element.Value.CoordY + element.Value.Height);

                        if (selectRect1.X <= elementRect1.X && selectRect1.Y <= elementRect1.Y
                            && selectRect2.X >= elementRect2.X && selectRect2.Y >= elementRect2.Y)
                        {
                            //if (!VM.SelectedElements.Contains(element.Value))
                            //if (VM.SelectedElements.Find(x => x.Id == element.Value.Id) is null)
                            if (VM.SelectedElements.FirstOrDefault(x => x.Id == element.Value.Id) is null)
                            {
                                SelectElement(element.Value);
                            }
                        }
                        else
                        {
                            //if (VM.SelectedElements.Contains(element.Value))
                            //if (VM.SelectedElements.Find(x => x.Id == element.Value.Id) is not null)
                            if (VM.SelectedElements.FirstOrDefault(x => x.Id == element.Value.Id) is not null)
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
                        Point elementRect2 = new Point(element.Value.CoordX + element.Value.Width, element.Value.CoordY + element.Value.Height);

                        if (selectRect2.X >= elementRect1.X && selectRect2.Y >= elementRect1.Y &&
                            selectRect1.X <= elementRect2.X && selectRect1.Y <= elementRect2.Y)
                        {
                            //if (!VM.SelectedElements.Contains(element.Value))
                            //if (VM.SelectedElements.Find(x => x.Id == element.Value.Id) is null)
                            if (VM.SelectedElements.FirstOrDefault(x => x.Id == element.Value.Id) is null)
                            {
                                SelectElement(element.Value);
                            }
                        }
                        else
                        {
                            //if (VM.SelectedElements.Contains(element.Value))
                            //if (VM.SelectedElements.Find(x => x.Id == element.Value.Id) is not null)
                            if (VM.SelectedElements.FirstOrDefault(x => x.Id == element.Value.Id) is not null)
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
            //VM.SelectedElements.ForEach(x => x.HideResizeBorder());
            foreach (var item in VM.SelectedElements)
            {
                item.HideResizeBorder();
            }
            //VM.SelectedElements.ForEach(x => x.ShowMovingBorder());
            foreach (var item in VM.SelectedElements)
            {
                item.ShowMovingBorder();
            }
        }

        void SelectedElementsToResizingMode()
        {
            //VM.SelectedElements.ForEach(x => x.HideMovingBorder());
            foreach (var item in VM.SelectedElements)
            {
                item.HideMovingBorder();
            }
            //VM.SelectedElements.ForEach(x => x.ShowResizeBorder());
            foreach (var item in VM.SelectedElements)
            {
                item.ShowResizeBorder();
            }
        }



        void DeselectOneElement(ScreenElement element)
        {
            // find border of this element and delete it
            // remove this element from the list
            // create version for string as argument?

            //if (!VM.SelectedElements.Contains(element))
            //if (VM.SelectedElements.Find(x => x.Id == element.Id) is null)
            if (VM.SelectedElements.FirstOrDefault(x => x.Id == element.Id) is null)
            {
                return;
            }

            VM.SelectedElements.Remove(element);
            element.HideResizeBorder();
        }


        internal (double x, double y) FindPlaceForNewElement(ScreenElement element)
        {
            double centerX = this.Width / 2;
            double centerY = this.Height / 2;

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

                    if (newCenterX + element.Width >= this.Width)
                    {
                        newCenterX = this.Width - element.Width;
                        endX = true;
                    }

                    if (newCenterY + element.Height >= this.Height)
                    {
                        newCenterY = this.Height - element.Height;
                        endY = true;
                    }

                    centerX = newCenterX < 0 ? 0 : newCenterX;
                    centerY = newCenterY < 0 ? 0 : newCenterY;
                }
            }

            return (centerX, centerY);

        }

        internal void ChangeCoorditanesOnMoving(ObservableCollection<ScreenElement> movingElements,
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
                if (currentPosition.X >= this.Width || currentPosition.X <= 0
                   || currentPosition.Y >= this.Height || currentPosition.Y <= 0)
                {
                    return;
                }
            }
            else
            {
                foreach (var element in movingElements)
                {
                    if (element.CoordX + offsetX + element.Width >= this.Width
                        || element.CoordY + offsetY + element.Height >= this.Height
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
                Canvas.SetLeft(element, element.ZoomedCoordX);
                Canvas.SetTop(element, element.ZoomedCoordY);
            }
        }


        internal void CreateTmpElementsWithOpacity(ObservableCollection<ScreenElement> copyFromElements)
        {
            for (int i = 0; i < copyFromElements.Count; i++)
            {
                var type = copyFromElements[i].GetType();
                var tmpElement = (ScreenElement)Activator.CreateInstance(type, copyFromElements[i].ElementContent);
                tmpElement.InitializeFromAnotherElement(copyFromElements[i]);
                tmpElement.Id = -9999 + i;
                tmpElement.Name = $"TMP_FOLLOWER_{i}";
                tmpElement.Opacity = 0.5;
                tmpElement.CoordX = copyFromElements[i].CoordX;
                tmpElement.CoordY = copyFromElements[i].CoordY;
                tmpElement.ZoomCoef = ZoomCoef;

                tmpElement.MouseLeftButtonUp += Element_MouseLeftButtonUp;

                TmpFollowerElements.Add(tmpElement);

                this.Children.Add(tmpElement);
                Canvas.SetLeft(tmpElement, tmpElement.ZoomedCoordX);
                Canvas.SetTop(tmpElement, tmpElement.ZoomedCoordY);
            }
        }



        internal void RemoveTmpElementsWithOpacity()
        {
            //TmpFollowerElements.ForEach(x => this.Children.Remove(x));
            foreach (var item in TmpFollowerElements)
            {
                this.Children.Remove(item);
            }
            TmpFollowerElements.Clear();
        }


        internal bool CanMoveElements(ObservableCollection<ScreenElement> elementsToMove, Key direction)
        {
            bool canMove = true;
            foreach (var element in elementsToMove)
            {
                switch (direction)
                {
                    case Key.Right:
                        if (element.CoordX + element.Width >= this.Width)
                        {
                            canMove = false;
                        }
                        break;
                    case Key.Down:
                        if (element.CoordY + element.Height >= this.Height)
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

            var unzoomedViewportWidth = ParentalScroller.ViewportWidth / ZoomCoef;
            var unzoomedViewportHeight = ParentalScroller.ViewportHeight / ZoomCoef;
            var unzoomedHorizontalOffset = ParentalScroller.HorizontalOffset / ZoomCoef;
            var unzoomedVerticalOffset = ParentalScroller.VerticalOffset / ZoomCoef;

            // TODO complete it with backmoving
            //if (currentMousePosition.X >= ParentalScroller.ViewportWidth - ParentalScroller.HorizontalOffset - 10)
            //{
            //    ParentalScroller.ScrollToHorizontalOffset(ParentalScroller.HorizontalOffset + 0.5);
            //}

            //if (currentMousePosition.Y >= ParentalScroller.ViewportHeight - ParentalScroller.VerticalOffset - 10)
            //{
            //    ParentalScroller.ScrollToVerticalOffset(ParentalScroller.VerticalOffset + 0.5);
            //}



            if (currentMousePosition.X >= unzoomedViewportWidth + unzoomedHorizontalOffset - 10)
            {
                ParentalScroller.ScrollToHorizontalOffset(ParentalScroller.HorizontalOffset + 0.5);
            }

            if (currentMousePosition.Y >= unzoomedViewportHeight + unzoomedVerticalOffset - 10)
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



        public ElementProperty<T> CreateEditableProperty<T>(
            string propertyName,
            string description,
            //ScreenElement elementWithProperties,
            Func<T, string> validation = null,
            bool canConnectSignal = false,
            bool editable = true,
            string customName = null
            )
        {
            ElementProperty<T> newProperty = new ElementProperty<T>(
                customName is null ? propertyName : customName,
                description,
                validation,
                canConnectSignal,
                editable);

            // Events
            // This event invokes only if value was changed - to avoid stackoverflow
            newProperty.PropertyChangedNotEqual += (sender, e) =>
            {
                if (e.PropertyName == "Value")
                {
                    this.GetType().GetProperty(propertyName).SetValue(this, ((ElementProperty<T>)sender).Value);
                }
            };

            this.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == propertyName)
                {
                    newProperty.Value = (T)sender.GetType().GetProperty(propertyName).GetValue(sender, null);
                }
            };

            // set on element 

            //ElementProperties.Add(newProperty);
            return newProperty;

        }

        public static Point UnzoomCoordinates(Point zoomedCoordinates, double zoomCoef)
        {
            return new Point(zoomedCoordinates.X / zoomCoef, zoomedCoordinates.Y / zoomCoef);
        }

        public static Point ZoomCoordinates(Point zoomedCoordinates, double zoomCoef)
        {
            return new Point(zoomedCoordinates.X * zoomCoef, zoomedCoordinates.Y * zoomCoef);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

    }
}
