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


        public double CoordX { get; set; }
        public double CoordY { get; set; }
        public MouseSelectingDirection currentSelectingDirection;

        public ElementsSelectingBorder()
        {
            InitializeComponent();
        }

        public void AddBorderOnWorkspace(string name, Canvas workspace, Point startPoint)
        {
            // first time we must add this border on the workspace. User just started selection (left click + 2-3 pixel moving or so)
            this.startPoint = startPoint;
            this.Name = name;
            this.Width = 1;
            this.Height = 1;
            workspace.Children.Add(this);
            Canvas.SetLeft(this, startPoint.X);
            Canvas.SetTop(this, startPoint.Y);

            widthOfWorkspace = workspace.ActualWidth;
            heightOfWorkspace = workspace.ActualHeight;

            CoordX = startPoint.X;
            CoordY = startPoint.Y;

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
                    Canvas.SetLeft(this, currentPoint.X);
                    CoordX = currentPoint.X;
                    this.Width = Math.Abs(currentPoint.X - startPoint.X);
                    this.Height = currentPoint.Y - startPoint.Y;
                    currentSelectingDirection = MouseSelectingDirection.LeftDown;
                }
                // Direction - left-up
                else if (currentPoint.X - startPoint.X < 0 && currentPoint.Y - startPoint.Y < 0)
                {
                    Canvas.SetLeft(this, currentPoint.X);
                    CoordX = currentPoint.X;
                    Canvas.SetTop(this, currentPoint.Y);
                    CoordY = currentPoint.Y;
                    this.Width = Math.Abs(currentPoint.X - startPoint.X);
                    this.Height = Math.Abs(currentPoint.Y - startPoint.Y);
                    currentSelectingDirection = MouseSelectingDirection.LeftUp;
                }
                // Direction - right-up
                else if (currentPoint.X - startPoint.X >= 0 && currentPoint.Y - startPoint.Y < 0)
                {
                    Canvas.SetTop(this, currentPoint.Y);
                    CoordY = currentPoint.Y;
                    this.Width = currentPoint.X - startPoint.X;
                    this.Height = Math.Abs(currentPoint.Y - startPoint.Y);
                    currentSelectingDirection = MouseSelectingDirection.RightUp;
                }





            }
        }

    }

    
}
