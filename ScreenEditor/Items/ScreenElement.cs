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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ExpandScadaEditor.ScreenEditor.WorkspaceHelperControls;
using ExpandScadaEditor.ScreenEditor.Items.Properties;
using System.Reflection;

namespace ExpandScadaEditor.ScreenEditor.Items
{

    /*      Properties
     *      
     *  
     *  - create dictionary or observable collection to collect these properties. Try to make it not so ugly
     *  - prepare some view for adequate showing of these properties on editor
     *      - create listview for all parameters. 
     *      - if there are some groups - we can split them on groups like in VS
     *      - in the bottom - show description to each parameter if it is selected
     * 
     * 
     * 
     * 
     * 
     * 
     * 
     * 
     * */

    public class ScreenElement : UserControl, INotifyPropertyChanged
    {
        public const string RESIZE_BORDER_NAME = "RESIZE_BORDER";
        public const string COVER_BORDER_NAME = "COVER_BORDER";
        public const string MOVING_BORDER_NAME = "MOVING_BORDER";

        public static Func<double, string> positiveDoubleValidation = delegate (double val)
        {
            if (val < 0)
                return "Must be positive";
            return String.Empty;
        };


        //public ObservableCollection<ElementProperty> ElementProperties = new ObservableCollection<ElementProperty>()
        //{
        //    new ElementProperty<string>("ID", "Id of element"),
        //    new ElementProperty<string>("Name", "Name of the element"),
        //    new ElementProperty<double>("Width", "Width in px"),
        //    new ElementProperty<double>("Height", "Heifht in px"),
        //    new ElementProperty<double>("X", "Coordinate X"),
        //    new ElementProperty<double>("Y", "Coordinate Y"),


        //};

        private ObservableCollection<ElementProperty> elementProperties = new ObservableCollection<ElementProperty>();
        public ObservableCollection<ElementProperty> ElementProperties
        {
            get
            {
                return elementProperties;
            }
            set
            {
                elementProperties = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ElementProperty<double>> elementPropertiesTest = new ObservableCollection<ElementProperty<double>>();
        public ObservableCollection<ElementProperty<double>> ElementPropertiesTest
        {
            get
            {
                return elementPropertiesTest;
            }
            set
            {
                elementPropertiesTest = value;
                OnPropertyChanged();
            }
        }

        //private double testProp1;
        //public double TestProp1
        //{
        //    get
        //    {
        //        return testProp1;
        //    }
        //    set
        //    {
        //        testProp1 = value;
        //        //NotifyPropertyChanged();
        //    }
        //}




        //public event EventHandler<ResizingEventArgs> OnElementResizing;
        public event EventHandler ElementSizeChanged;

        private int id;
        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
                OnPropertyChanged();
            }
        }

        private double zoomedCcoordX;
        public double ZoomedCoordX
        {
            get
            {
                return zoomedCcoordX;
            }
            set
            {
                zoomedCcoordX = value;
                //NotifyPropertyChanged();
            }
        }

        private double zoomedCcoordY;
        public double ZoomedCoordY
        {
            get
            {
                return zoomedCcoordY;
            }
            set
            {
                zoomedCcoordY = value;
                //NotifyPropertyChanged();
            }
        }

        private double coordX;
        public double CoordX
        {
            get
            {
                return coordX;
            }
            set
            {
                coordX = value;
                ZoomedCoordX = coordX * ZoomCoef;
                OnPropertyChanged();
            }
        }

        private double coordY;
        public double CoordY
        {
            get
            {
                return coordY;
            }
            set
            {
                coordY = value;
                ZoomedCoordY = coordY * ZoomCoef;
                OnPropertyChanged();
            }
        }

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

                zoomCoef = value;

                base.Width = Width * zoomCoef;
                base.Height = Height * zoomCoef;

                ZoomedCoordX = CoordX * zoomCoef;
                ZoomedCoordY = CoordY * zoomCoef;
                Canvas.SetLeft(this, this.ZoomedCoordX);
                Canvas.SetTop(this, this.ZoomedCoordY);

                //NotifyPropertyChanged();
            }
        }

        private double workspaceHeight;
        public double WorkspaceHeight
        {
            get
            {
                return workspaceHeight;
            }
            set
            {
                workspaceHeight = value;
                //NotifyPropertyChanged();
            }
        }

        private double workspaceWidth;
        public double WorkspaceWidth
        {
            get
            {
                return workspaceWidth;
            }
            set
            {
                workspaceWidth = value;
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
                OnPropertyChanged();
            }
        }

        private bool catalogMode;
        public bool CatalogMode
        {
            get
            {
                return catalogMode;
            }
            set
            {
                catalogMode = value;
                //NotifyPropertyChanged();
            }
        }

        public void ShowResizeBorder()
        {
            try
            {
                var resizeBorder = (ElementResizingBorder)this.FindName(RESIZE_BORDER_NAME);
                if (resizeBorder != null)
                {
                    
                    resizeBorder.Visibility = Visibility.Visible;
                    if (!CatalogMode)
                    {
                        Cursor = Cursors.SizeAll;
                    }
                    
                }
                
            }
            catch (Exception ex)
            {
                // TODO ADD TO LOG   
            }

        }

        public void HideResizeBorder()
        {
            try
            {
                var resizeBorder = (ElementResizingBorder)this.FindName(RESIZE_BORDER_NAME);
                if (resizeBorder != null)
                {
                    resizeBorder.Visibility = Visibility.Hidden;
                    if (!CatalogMode)
                    {
                        Cursor = Cursors.Arrow;
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO ADD TO LOG   
            }

        }

        public void ShowCoverBorder()
        {
            try
            {
                var coverBorder = (MouseOverElementBorder)this.FindName(COVER_BORDER_NAME);
                if (coverBorder != null)
                {

                    coverBorder.Visibility = Visibility.Visible;
                }

            }
            catch (Exception ex)
            {
                // TODO ADD TO LOG   
            }

        }

        public void HideCoverBorder()
        {
            try
            {
                var coverBorder = (MouseOverElementBorder)this.FindName(COVER_BORDER_NAME);
                if (coverBorder != null)
                {
                    coverBorder.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                // TODO ADD TO LOG   
            }

        }


        public void ShowMovingBorder()
        {
            try
            {
                var movingBorder = (ElementMovingBorder)this.FindName(MOVING_BORDER_NAME);
                if (movingBorder != null)
                {

                    movingBorder.Visibility = Visibility.Visible;
                }

            }
            catch (Exception ex)
            {
                // TODO ADD TO LOG   
            }
        }

        public void HideMovingBorder()
        {
            try
            {
                var movingBorder = (ElementMovingBorder)this.FindName(MOVING_BORDER_NAME);
                if (movingBorder != null)
                {
                    movingBorder.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception ex)
            {
                // TODO ADD TO LOG   
            }
        }


        public ScreenElement()
        {
            Initialized += ScreenElement_Initialized;
            MouseEnter += ScreenElement_MouseEnter;
            MouseLeave += ScreenElement_MouseLeave;


            CreateEditableProperties();





            //Binding myBinding = new Binding("TestProp1");
            //myBinding.Source = this;
            //BindingOperations.SetBinding(this, WidthProperty, myBinding);

            //TODO 
            //Find a normal way to use it in my property class. There should be possibility
            //not only listen and change. Test changing (write it in another property e.g.)
            //Then try to initialize it pretty comfortable.

            //WeakReference wr = new WeakReference(this);
            //PropertyChangeNotifier notifier = new PropertyChangeNotifier(this, "Width");
            //notifier.ValueChanged += new EventHandler(OnMyValueChanged);

        }

        public void InitializeFromAnotherElement(ScreenElement element)
        {
            // TODO Expand this method if add new category of settings/ settig

            // TODO!!! These settings must be united to collections in future!!!
            id = element.id;
            CoordX = element.CoordX;
            CoordY = element.CoordY;

            // TODO is it really necessary to use Actual properties?
            Width = element.IsLoaded ? element.ActualWidth / element.zoomCoef : element.Width;
            Height = element.IsLoaded ? element.ActualHeight / element.zoomCoef : element.Height;
            // Zooming experiments
            //Width = element.Width;
            //Height = element.Height;

            Name = element.Name;
           // ToDelete = element.ToDelete;
            
        }


        private void ScreenElement_MouseLeave(object sender, MouseEventArgs e)
        {
            HideCoverBorder();
        }

        private void ScreenElement_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowCoverBorder();
        }

        private void ScreenElement_Initialized(object sender, EventArgs e)
        {
            // Just WPf hacking....
            HideResizeBorder();
            HideCoverBorder();

            //resizing events
            try
            {

                var resizeBorder = (ElementResizingBorder)this.FindName(RESIZE_BORDER_NAME);
                if (resizeBorder != null)
                {
                    resizeBorder.SizeRight.MouseMove += SizeRight_MouseMove;
                    resizeBorder.SizeBottom.MouseMove += SizeBottom_MouseMove;
                    resizeBorder.SizeTop.MouseMove += SizeTop_MouseMove;
                    resizeBorder.SizeLeft.MouseMove += SizeLeft_MouseMove;
                    resizeBorder.SizeNWSE_leftUp.MouseMove += SizeNWSE_leftUp_MouseMove;
                    resizeBorder.SizeNESW_rightUp.MouseMove += SizeNESW_rightUp_MouseMove;
                    resizeBorder.SizeNESW_leftDown.MouseMove += SizeNESW_leftDown_MouseMove;
                    resizeBorder.SizeNWSE_rightDown.MouseMove += SizeNWSE_rightDown_MouseMove;
                        
                    resizeBorder.SizeRight.MouseLeftButtonDown += Resizing_MouseLeftButtonDown;
                    resizeBorder.SizeRight.MouseLeftButtonUp += Resizing_MouseLeftButtonUp;
                    resizeBorder.SizeBottom.MouseLeftButtonDown += Resizing_MouseLeftButtonDown;
                    resizeBorder.SizeBottom.MouseLeftButtonUp += Resizing_MouseLeftButtonUp;
                    resizeBorder.SizeTop.MouseLeftButtonDown += Resizing_MouseLeftButtonDown;
                    resizeBorder.SizeTop.MouseLeftButtonUp += Resizing_MouseLeftButtonUp;
                    resizeBorder.SizeLeft.MouseLeftButtonDown += Resizing_MouseLeftButtonDown;
                    resizeBorder.SizeLeft.MouseLeftButtonUp += Resizing_MouseLeftButtonUp;
                    resizeBorder.SizeNWSE_leftUp.MouseLeftButtonDown += Resizing_MouseLeftButtonDown;
                    resizeBorder.SizeNWSE_leftUp.MouseLeftButtonUp += Resizing_MouseLeftButtonUp;
                    resizeBorder.SizeNESW_rightUp.MouseLeftButtonDown += Resizing_MouseLeftButtonDown;
                    resizeBorder.SizeNESW_rightUp.MouseLeftButtonUp += Resizing_MouseLeftButtonUp;
                    resizeBorder.SizeNESW_leftDown.MouseLeftButtonDown += Resizing_MouseLeftButtonDown;
                    resizeBorder.SizeNESW_leftDown.MouseLeftButtonUp += Resizing_MouseLeftButtonUp;
                    resizeBorder.SizeNWSE_rightDown.MouseLeftButtonDown += Resizing_MouseLeftButtonDown;
                    resizeBorder.SizeNWSE_rightDown.MouseLeftButtonUp += Resizing_MouseLeftButtonUp;
                }
            }
            catch (Exception ex)
            {
                // TODO ADD TO LOG   
            }
        }

        private void SizeNWSE_rightDown_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                var mousePosition = UnzoomCoordinates(e.GetPosition(this));
                ActualizeResizingOnWorkspace(new ResizingEventArgs
                {
                    ResizingType = ResizingType.ChangeSize,
                    NewWidth = mousePosition.X > 0 ? mousePosition.X : 1,
                    NewHeight = mousePosition.Y > 0 ? mousePosition.Y : 1
                });
            }
        }

        private void SizeNESW_leftDown_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                var mousePosition = UnzoomCoordinates(e.GetPosition(this));
                double offsetX = mousePosition.X;

                if (this.Width - offsetX > 0 && this.CoordX + offsetX >= 0)
                {
                    this.CoordX = this.CoordX + offsetX;
                    this.Width = this.Width - offsetX;
                }

                ActualizeResizingOnWorkspace(new ResizingEventArgs { 
                    ResizingType = ResizingType.ChangeSize,
                    NewHeight = mousePosition.Y > 0 ? mousePosition.Y : 1
                });
            }
        }

        private void SizeNESW_rightUp_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                var mousePosition = UnzoomCoordinates(e.GetPosition(this));
                double offsetY = mousePosition.Y;

                if (this.Height - offsetY > 0 && this.CoordY + offsetY >= 0)
                {
                    this.CoordY = this.CoordY + offsetY;
                    this.Height = this.Height - offsetY;
                }
                ActualizeResizingOnWorkspace(new ResizingEventArgs { 
                    ResizingType = ResizingType.ChangeSize,
                    NewWidth = mousePosition.X > 0 ? mousePosition.X : 1
                });
            }
        }

        private void SizeNWSE_leftUp_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                var mousePosition = UnzoomCoordinates(e.GetPosition(this));
                double offsetX = mousePosition.X;
                double offsetY = mousePosition.Y;
                if (this.Width - offsetX > 0 && this.CoordX + offsetX >= 0)
                {
                    this.CoordX = this.CoordX + offsetX;
                    this.Width = this.Width - offsetX;
                }

                if (this.Height - offsetY > 0 && this.CoordY + offsetY >= 0)
                {
                    this.CoordY = this.CoordY + offsetY;
                    this.Height = this.Height - offsetY;
                }
                ActualizeResizingOnWorkspace(new ResizingEventArgs { ResizingType = ResizingType.ChangeSize });
            }
        }

        private void SizeLeft_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                var mousePosition = UnzoomCoordinates(e.GetPosition(this));
                double offsetX = mousePosition.X;
                
                if (this.Width - offsetX > 0 && this.CoordX + offsetX >= 0)
                {
                    this.CoordX = this.CoordX + offsetX;
                    this.Width = this.Width - offsetX;
                }
                ActualizeResizingOnWorkspace(new ResizingEventArgs {ResizingType = ResizingType.ChangeSize });
            }
        }

        private void SizeTop_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                var mousePosition = UnzoomCoordinates(e.GetPosition(this));
                double offsetY = mousePosition.Y;
                if (this.Height - offsetY > 0 && this.CoordY + offsetY >= 0)
                {
                    this.CoordY = this.CoordY + offsetY;
                    this.Height = this.Height - offsetY;
                }
                ActualizeResizingOnWorkspace(new ResizingEventArgs { ResizingType = ResizingType.ChangeSize });
            }
        }

        private void SizeBottom_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                var mousePosition = UnzoomCoordinates(e.GetPosition(this));
                ActualizeResizingOnWorkspace(new ResizingEventArgs
                {
                    ResizingType = ResizingType.ChangeSize,
                    //NewWidth = this.ActualWidth,
                    NewHeight = mousePosition.Y > 0 ? mousePosition.Y : 1
                });
            }
        }

        private void SizeRight_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                var mousePosition = UnzoomCoordinates(e.GetPosition(this));
                ActualizeResizingOnWorkspace(new ResizingEventArgs
                {
                    ResizingType = ResizingType.ChangeSize,
                    NewWidth = mousePosition.X > 0 ? mousePosition.X : 1,
                    //NewHeight = this.ActualHeight
                });
            }
        }

        // this is used to be a event, but not anymore...
        void ActualizeResizingOnWorkspace(ResizingEventArgs e)
        {
            
            switch (e.ResizingType)
            {
                case ResizingType.ChangeSize:
                    if (!double.IsNaN(e.NewWidth) && e.NewWidth + this.CoordX < WorkspaceWidth)
                    {
                        Width = e.NewWidth;
                    }
                    if (!double.IsNaN(e.NewHeight) && e.NewHeight + this.CoordY < WorkspaceHeight)
                    {
                        Height = e.NewHeight;
                    }
                    Canvas.SetLeft(this, this.ZoomedCoordX);
                    Canvas.SetTop(this, this.ZoomedCoordY);
                    break;

            }
        }



        private void Resizing_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var shape = sender as Shape;
            shape.ReleaseMouseCapture();
            e.Handled = true;
            ElementSizeChanged(this, new EventArgs());
        }

        private void Resizing_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var shape = sender as Shape;
            shape.CaptureMouse();
            e.Handled = true;
        }

     //   private void ChangeZoom(double scaleCoef)
     //   {

     //       /*
     //        * - Changing properties
     //*              - Width/Height
     //*              - X/Y coordinates
     //*              - Text font
     //*              - Line thickness? 
     //*              - Resize borders (???)
     //        * */

     //       //ZoomCoef = scaleCoef;

     //       base.Width = Width * scaleCoef;
     //       base.Height = Height * scaleCoef;

     //       ZoomedCoordX = CoordX * scaleCoef;
     //       ZoomedCoordY = CoordY * scaleCoef;
     //       Canvas.SetLeft(this, this.ZoomedCoordX);
     //       Canvas.SetTop(this, this.ZoomedCoordY);

     //   }

        Point UnzoomCoordinates(Point zoomedCoordinates)
        {
            return new Point(zoomedCoordinates.X / ZoomCoef, zoomedCoordinates.Y / ZoomCoef);
        }



        public void AddEditableProperty<T>(string propertyName,
            string description,
            Func<T, string> validation = null,
            bool canConnectSignal = false,
            bool editable = true,
            string customName = null
            )
        {
            var propertyInfo = this.GetType().GetProperty(propertyName);

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
                    this.GetType().GetProperty(propertyName).SetValue(this, ((ElementProperty)sender).Value);
                }
            };

            this.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == propertyName)
                {
                    newProperty.Value = sender.GetType().GetProperty(propertyName).GetValue(sender, null);
                }
            };

            // set on element 

            ElementProperties.Add(newProperty);

        }

        public void AddEditablePropertyTest(string propertyName,
            string description,
            Func<double, string> validation = null,
            bool canConnectSignal = false,
            bool editable = true,
            string customName = null
            )
        {
            var propertyInfo = this.GetType().GetProperty(propertyName);

            ElementProperty<double> newProperty = new ElementProperty<double>(
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
                    this.GetType().GetProperty(propertyName).SetValue(this, ((ElementProperty)sender).Value);
                }
            };

            this.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == propertyName)
                {
                    newProperty.Value = sender.GetType().GetProperty(propertyName).GetValue(sender, null);
                }
            };

            // set on element 

            ElementPropertiesTest.Add(newProperty);

        }



        private void CreateEditableProperties()
        {
            //AddEditableProperty<int>(nameof(Id), "Id of element", editable: false, customName: "ID");
            //AddEditableProperty<string>(nameof(Name), "Name of the element");
            //AddEditableProperty<double>(nameof(Height), "Height in px", positiveDoubleValidation);
            //AddEditableProperty<double>(nameof(Width), "Width in px", positiveDoubleValidation);
            //AddEditableProperty<double>(nameof(CoordX), "Coordinate X", customName: "X");
            //AddEditableProperty<double>(nameof(CoordY), "Coordinate Y", customName: "Y");

            //AddEditableProperty<int>(nameof(Id), "Id of element", editable: false, customName: "ID");
            //AddEditableProperty<string>(nameof(Name), "Name of the element");
            AddEditablePropertyTest(nameof(Height), "Height in px", positiveDoubleValidation);
            AddEditablePropertyTest(nameof(Width), "Width in px", positiveDoubleValidation);
            AddEditablePropertyTest(nameof(CoordX), "Coordinate X", customName: "X");
            AddEditablePropertyTest(nameof(CoordY), "Coordinate Y", customName: "Y");


        }







        static bool PropertiesAreEqual(ScreenElement elementA, ScreenElement elementB)
        {
            // TODO put here list of properties!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //      now use only for tests!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!


            if (elementA is null && elementB is null)
            {
                return true;
            }
            else if (elementA is not null && elementB is null || elementA is null && elementB is not null)
            {
                return false;
            }

            return elementA.id == elementB.id && elementA.Name == elementB.Name
            && elementA.coordX == elementB.coordX && elementA.coordY == elementB.coordY
            && elementA.Width == elementB.Width && elementA.Height == elementB.Height;
            //&& elementA.ActualWidth == elementB.ActualWidth && elementA.ActualHeight == elementB.ActualHeight;
        }

        public static bool operator ==(ScreenElement elementA, ScreenElement elementB)
        {

            return PropertiesAreEqual(elementA, elementB);
        }

        public static bool operator !=(ScreenElement elementA, ScreenElement elementB)
        {
            return !PropertiesAreEqual(elementA, elementB);
        }



        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }




    }
}
