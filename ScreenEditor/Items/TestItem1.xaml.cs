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

namespace ExpandScadaEditor.ScreenEditor.Items
{
    /// <summary>
    /// Логика взаимодействия для TestItem1.xaml
    /// </summary>
    public partial class TestItem1 : UserControl
    {
        protected TestItem1VM ViewModel
        {
            get { return (TestItem1VM)Resources["ViewModel"]; }
        }

        public TestItem1()
        {
            InitializeComponent();
        }


        private Point _positionInBlock;
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.ChangedButton == MouseButton.Left)
            {
                //this.CaptureMouse();

                //isDragging = true;

                var container = VisualTreeHelper.GetParent(this) as UIElement;
                if (container == null)
                    return;

                var mousePosition = e.GetPosition(container);
                ViewModel.CoordX = mousePosition.X;
                ViewModel.CoordY = mousePosition.Y;
                ViewModel.IsDragged = true;

            }
        }


        //protected override void OnMouseMove(MouseEventArgs e)
        //{
        //    base.OnMouseMove(e);
        //    if (this.IsMouseCaptured /*e.LeftButton == MouseButtonState.Pressed*/)
        //    {
        //        // Package the data.
        //        DataObject data = new DataObject();
        //        data.SetData(ViewModel);

        //        // Initiate the drag-and-drop operation.

        //        DragDropEffects dragDropEffects;
        //        if (ViewModel.StoredInCatalog)
        //        {
        //            dragDropEffects = DragDropEffects.Copy;
        //        }
        //        else
        //        {
        //            dragDropEffects = DragDropEffects.Copy | DragDropEffects.Move;
        //        }

        //        // follow the mouse 
        //        //var upperlimit = canvPosToWindow.Y + (r.Height / 2);
        //        //var lowerlimit = canvPosToWindow.Y + canv.ActualHeight - (r.Height / 2);

        //        //var leftlimit = canvPosToWindow.X + (r.Width / 2);
        //        //var rightlimit = canvPosToWindow.X + canv.ActualWidth - (r.Width / 2);


        //        //var absmouseXpos = e.GetPosition(this).X;
        //        //var absmouseYpos = e.GetPosition(this).Y;

        //        //if ((absmouseXpos > leftlimit && absmouseXpos < rightlimit)
        //        //    && (absmouseYpos > upperlimit && absmouseYpos < lowerlimit))
        //        //{
        //        //    Canvas.SetLeft(r, e.GetPosition(canv).X - (r.Width / 2));
        //        //    Canvas.SetTop(r, e.GetPosition(canv).Y - (r.Height / 2));
        //        //}



        //        // get the parent container
        //        var container = VisualTreeHelper.GetParent(this) as UIElement;

        //        if (container == null)
        //            return;

        //        // get the position within the container
        //        var mousePosition = e.GetPosition(container);


        //        DragDrop.DoDragDrop(this, data, dragDropEffects);


        //        // move the usercontrol.
        //        this.RenderTransform = new TranslateTransform(mousePosition.X - _positionInBlock.X, mousePosition.Y - _positionInBlock.Y);















        //    }
        //}


        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            //this.ReleaseMouseCapture();
            //isDragging = false;
            ViewModel.IsDragged = false;
        }
    }
}
