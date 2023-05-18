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
    /// Логика взаимодействия для ElementsSelectedBorder.xaml
    /// </summary>
    public partial class ElementsSelectedBorder : UserControl
    {
        public ElementsSelectedBorder()
        {
            InitializeComponent();
        }

        public double CoordX { get; set; }
        public double CoordY { get; set; }

        public void AddBorderOnWorkspace(string name, List<ScreenElement> selectedElements, Canvas workspace)
        {
            // just add the border with invisible sizes
            Point startPoint = new Point(0, 0);
            if (selectedElements != null && selectedElements.Count > 0)
            {
                startPoint.X = selectedElements[0].CoordX;
                startPoint.Y = selectedElements[0].CoordY;
            }

            this.Name = name;
            this.Width = 0;
            this.Height = 0;
            workspace.Children.Add(this);
            Canvas.SetLeft(this, startPoint.X);
            Canvas.SetTop(this, startPoint.Y);
        }

        public void ActualizeSelectedBorderSizes(List<ScreenElement> selectedElements)
        {
            // calculate coordinates of all elements and find position and size for this border
            if (selectedElements == null || selectedElements.Count == 0)
            {
                this.Visibility = Visibility.Hidden;
                return;
            }

            Point point1 = new Point(double.NaN, double.NaN);
            Point point2 = new Point(double.NaN, double.NaN);

            foreach (var element in selectedElements)
            {
                // find minimal left-up position of all elements
                if (double.IsNaN(point1.X) || point1.X > element.CoordX)
                {
                    point1.X = element.CoordX;
                }

                if (double.IsNaN(point1.Y) || point1.Y > element.CoordY)
                {
                    point1.Y = element.CoordY;
                }

                // maximal right-down point of all elements
                if (double.IsNaN(point2.X) || point2.X < element.CoordX + element.ActualWidth)
                {
                    point2.X = element.CoordX + element.ActualWidth;
                }

                if (double.IsNaN(point2.Y) || point2.Y < element.CoordY + element.ActualHeight)
                {
                    point2.Y = element.CoordY + element.ActualHeight;
                }
            }


            if (double.IsNaN(point1.X) || double.IsNaN(point1.Y) || double.IsNaN(point2.X) || double.IsNaN(point2.Y)
                || point2.X < point1.X || point2.Y < point1.Y)
            {
                this.Visibility = Visibility.Hidden;
                return;
            }

            this.Visibility = Visibility.Visible;

            Canvas.SetLeft(this, point1.X);
            Canvas.SetTop(this, point1.Y);
            this.CoordX = point1.X;
            this.CoordY = point1.Y;
            this.Width = point2.X - point1.X;
            this.Height = point2.Y - point1.Y;
        }


    }
}
