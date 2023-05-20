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
                var mousePosition = e.GetPosition(this);
                OnElementResizing(this, new ResizingEventArgs
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
                var mousePosition = e.GetPosition(this);
                double offsetX = mousePosition.X;

                if (this.ActualWidth - offsetX > 0 && this.coordX + offsetX >= 0)
                {
                    this.coordX = this.coordX + offsetX;
                    this.Width = this.ActualWidth - offsetX;
                }

                OnElementResizing(this, new ResizingEventArgs { 
                    ResizingType = ResizingType.ChangeSize,
                    NewHeight = mousePosition.Y > 0 ? mousePosition.Y : 1
                });
            }
        }

        private void SizeNESW_rightUp_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var mousePosition = e.GetPosition(this);
                double offsetY = mousePosition.Y;

                if (this.ActualHeight - offsetY > 0 && this.coordY + offsetY >= 0)
                {
                    this.coordY = this.coordY + offsetY;
                    this.Height = this.ActualHeight - offsetY;
                }
                OnElementResizing(this, new ResizingEventArgs { 
                    ResizingType = ResizingType.ChangeSize,
                    NewWidth = mousePosition.X > 0 ? mousePosition.X : 1
                });
            }
        }

        private void SizeNWSE_leftUp_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var mousePosition = e.GetPosition(this);
                double offsetX = mousePosition.X;
                double offsetY = mousePosition.Y;
                if (this.ActualWidth - offsetX > 0 && this.coordX + offsetX >= 0)
                {
                    this.coordX = this.coordX + offsetX;
                    this.Width = this.ActualWidth - offsetX;
                }

                if (this.ActualHeight - offsetY > 0 && this.coordY + offsetY >= 0)
                {
                    this.coordY = this.coordY + offsetY;
                    this.Height = this.ActualHeight - offsetY;
                }
                OnElementResizing(this, new ResizingEventArgs { ResizingType = ResizingType.ChangeSize });
            }
        }

        private void SizeLeft_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var mousePosition = e.GetPosition(this);
                double offsetX = mousePosition.X;
                
                if (this.ActualWidth - offsetX > 0 && this.coordX + offsetX >= 0)
                {
                    this.coordX = this.coordX + offsetX;
                    this.Width = this.ActualWidth - offsetX;
                }
                OnElementResizing(this, new ResizingEventArgs {ResizingType = ResizingType.ChangeSize });
            }
        }

        private void SizeTop_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var mousePosition = e.GetPosition(this);
                double offsetY = mousePosition.Y;
                if (this.ActualHeight - offsetY > 0 && this.coordY + offsetY >= 0)
                {
                    this.coordY = this.coordY + offsetY;
                    this.Height = this.ActualHeight - offsetY;
                }
                OnElementResizing(this, new ResizingEventArgs { ResizingType = ResizingType.ChangeSize });
            }
        }

        private void SizeBottom_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var mousePosition = e.GetPosition(this);
                OnElementResizing(this, new ResizingEventArgs
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
                var mousePosition = e.GetPosition(this);
                OnElementResizing(this, new ResizingEventArgs
                {
                    ResizingType = ResizingType.ChangeSize,
                    NewWidth = mousePosition.X > 0 ? mousePosition.X : 1,
                    //NewHeight = this.ActualHeight
                });
            }
        }

        private void Resizing_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var shape = sender as Shape;
            shape.ReleaseMouseCapture();
            e.Handled = true;
        }

        private void Resizing_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var shape = sender as Shape;
            shape.CaptureMouse();
            e.Handled = true;
        }

        
    }
}
