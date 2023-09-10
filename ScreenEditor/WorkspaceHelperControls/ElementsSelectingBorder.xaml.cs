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
using ExpandScadaEditor.ScreenEditor.Items;

namespace ExpandScadaEditor.ScreenEditor.WorkspaceHelperControls
{
    /// <summary>
    /// Логика взаимодействия для ElementsSelectingBorder.xaml
    /// </summary>
    public partial class ElementsSelectingBorder : UserControl
    {
        Point startPoint;
        double widthOfWorkspace;
        double heightOfWorkspace;
        bool selectionWasStarted;

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

                //base.Width = Width * zoomCoef;
                //base.Height = Height * zoomCoef;

                //ZoomedCoordX = CoordX * zoomCoef;
                //ZoomedCoordY = CoordY * zoomCoef;
                //Canvas.SetLeft(this, this.ZoomedCoordX);
                //Canvas.SetTop(this, this.ZoomedCoordY);
                //NotifyPropertyChanged();
            }
        }

        private double zoomedCoordX;
        public double ZoomedCoordX
        {
            get
            {
                return zoomedCoordX;
            }
            set
            {
                zoomedCoordX = value;
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
                //NotifyPropertyChanged();
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
                //NotifyPropertyChanged();
            }
        }
        public MouseSelectingDirection currentSelectingDirection;

        public ElementsSelectingBorder()
        {
            InitializeComponent();
        }

        public void AddBorderOnWorkspace(string name, WorkspaceCanvas workspace, Point startPoint)
        {
            // first time we must add this border on the workspace. User just started selection (left click + 2-3 pixel moving or so)
            
            this.startPoint = startPoint;
            this.Name = name;
            this.Width = 1;
            this.Height = 1;
            workspace.Children.Add(this);

            this.ZoomCoef = workspace.ZoomCoef;
            CoordX = startPoint.X;
            CoordY = startPoint.Y;

            Canvas.SetLeft(this, ZoomedCoordX);
            Canvas.SetTop(this, ZoomedCoordY);

            widthOfWorkspace = workspace.Width;
            heightOfWorkspace = workspace.Height;

            

            selectionWasStarted = true;
        }

        public void ContinueSelection(Point currentPoint)
        {
            // change size of the rectangle
            if (selectionWasStarted)
            {
                // Check limits of Workspace
                if (currentPoint.X > widthOfWorkspace)
                {
                    currentPoint.X = widthOfWorkspace;
                }
                if (currentPoint.Y > heightOfWorkspace)
                {
                    currentPoint.Y = heightOfWorkspace;
                }
                if (currentPoint.X < 0)
                {
                    currentPoint.X = 0;
                }
                if (currentPoint.Y < 0)
                {
                    currentPoint.Y = 0;
                }


                // Direction - right-down
                if (currentPoint.X - startPoint.X >= 0 && currentPoint.Y - startPoint.Y >= 0)
                {
                    this.Width = currentPoint.X - startPoint.X;
                    this.Height = currentPoint.Y - startPoint.Y;
                    currentSelectingDirection = MouseSelectingDirection.RightDown;
                }
                // Direction - left-down
                else if (currentPoint.X - startPoint.X < 0 && currentPoint.Y - startPoint.Y >= 0)
                {
                    CoordX = currentPoint.X;
                    Canvas.SetLeft(this, ZoomedCoordX);
                    
                    this.Width = Math.Abs(currentPoint.X - startPoint.X);
                    this.Height = currentPoint.Y - startPoint.Y;
                    currentSelectingDirection = MouseSelectingDirection.LeftDown;
                }
                // Direction - left-up
                else if (currentPoint.X - startPoint.X < 0 && currentPoint.Y - startPoint.Y < 0)
                {
                    CoordX = currentPoint.X;
                    Canvas.SetLeft(this, ZoomedCoordX);
                    CoordY = currentPoint.Y;
                    Canvas.SetTop(this, ZoomedCoordY);
                    
                    this.Width = Math.Abs(currentPoint.X - startPoint.X);
                    this.Height = Math.Abs(currentPoint.Y - startPoint.Y);
                    currentSelectingDirection = MouseSelectingDirection.LeftUp;
                }
                // Direction - right-up
                else if (currentPoint.X - startPoint.X >= 0 && currentPoint.Y - startPoint.Y < 0)
                {
                    CoordY = currentPoint.Y;
                    Canvas.SetTop(this, ZoomedCoordY);
                    
                    this.Width = currentPoint.X - startPoint.X;
                    this.Height = Math.Abs(currentPoint.Y - startPoint.Y);
                    currentSelectingDirection = MouseSelectingDirection.RightUp;
                }





            }
        }

    }

    
}
