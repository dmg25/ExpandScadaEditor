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


namespace ExpandScadaEditor.ScreenEditor.Items
{

    /* 
     *  - catch click event on each resizing element
     *  - subscribe here on each event and calculate offset for new size/position
     *  - after offset is calculated invoke event with result and workspace will change the element
     * 
     * */


    public class ScreenElement : UserControl
    {
        public const string RESIZE_BORDER_NAME = "RESIZE_BORDER";
        public const string COVER_BORDER_NAME = "COVER_BORDER";

        public event EventHandler<ResizingEventArgs> OnElementResizing;
        public event EventHandler StartResizing;
        public event EventHandler StopResizing;

        private bool isDragged;
        public bool IsDragged
        {
            get
            {
                return isDragged;
            }
            set
            {
                isDragged = value;
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
                    Cursor = Cursors.SizeAll;
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
                    Cursor = Cursors.Arrow;
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

         

        public ScreenElement()
        {
            Initialized += ScreenElement_Initialized;
            MouseEnter += ScreenElement_MouseEnter;
            MouseLeave += ScreenElement_MouseLeave;
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
            HideResizeBorder();
            HideCoverBorder();

            //resizing events
            try
            {
                var resizeBorder = (ElementResizingBorder)this.FindName(RESIZE_BORDER_NAME);
                if (resizeBorder != null)
                {
                    resizeBorder.SizeRight.MouseMove += SizeRight_MouseMove;
                    resizeBorder.SizeRight.MouseLeftButtonDown += SizeRight_MouseLeftButtonDown;
                    resizeBorder.SizeRight.MouseLeftButtonUp += SizeRight_MouseLeftButtonUp;
                }
            }
            catch (Exception ex)
            {
                // TODO ADD TO LOG   
            }
        }

        private void SizeRight_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var shape = sender as Shape;
            shape.ReleaseMouseCapture();
            e.Handled = true;
            // StopResizing(this, new EventArgs());
            // this.ReleaseMouseCapture();
        }

        private void SizeRight_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var shape = sender as Shape;
            shape.CaptureMouse();
            e.Handled = true;
            // StartResizing(this, new EventArgs());
           // this.CaptureMouse();
        }

        private void SizeRight_MouseMove(object sender, MouseEventArgs e)
        {
            //e.Handled = true;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var mousePosition = e.GetPosition(this);

                OnElementResizing(this, new ResizingEventArgs
                {
                    ResizingType = ResizingType.ChangeSize,
                    OffsetX = mousePosition.X - this.ActualWidth,
                    OffsetY = 0
                });

            }
            
        }
    }
}
