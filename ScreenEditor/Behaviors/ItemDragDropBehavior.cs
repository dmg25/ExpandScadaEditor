using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.IO;
using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Runtime.InteropServices;
using ExpandScadaEditor.ScreenEditor.Items;
using System.Windows.Shapes;

namespace ExpandScadaEditor.ScreenEditor.Behaviors
{

    /*      Drag and Drop path:
     *      
     *  1. Cursor is over the item  - MouseEnter/MouseLeave
     *      - show some thin border around of element on Enter and hide in Leave
     *  2. left button pressed      - MouseLeftButtonDown OR MouseDown
     *      - Check some conditions if we can start dragging of the element
     *      - copy to some buffer instance of this element (with coordinates)
     *      - remember coordinates of cursor above this item
     *  3. Moving                   - MouseMove
     *      - check if left button is still pressed
     *      - check if CTRL button pressed or not
     *          - if yes - this is copying, if not - moving. If not, but item was in List - copying
     *      - move this element to cursor's coordinates, and correct them, to move exact same point of element as it was in the beginning
     *      - if copying - opacity 0,5. If not - normal view
     *  4. Release item             - MouseLeftButtonUp OR MouseUp
     *      - check if drop place supports dropping
     *      - calculate coordinates according current dropping place
     *      - check if this is moving - just put the elemnt here, if copying - paste here copy of the instance, give new system name maybe too
     *  
     * */


    /*      LIST OF SHAME
     *  - MouseMove and DoDragDrop make a conflict to each other, but DoDragDrop wins
     *  - Creating cursor with IntPtr will not work because obsolete
     *  - Trying adapt to new cursor - failed, we can not use any format, only .cur
     *  - trying convert png to .cur doesn't work I DON'T KNOW WHY, JUST ERROR ON CREATING; WRONG FORMAT OR SMTH
     *  - use moving on feedback???
     * 
     * 
     * 
     * 
     * 
     * */




    public struct IconInfo
    {
        public bool fIcon;
        public int xHotspot;
        public int yHotspot;
        public IntPtr hbmMask;
        public IntPtr hbmColor;
    }



    //public class CursorUtil
    //{
    //    [DllImport("user32.dll")]
    //    public static extern IntPtr CreateIconIndirect(ref IconInfo icon);

    //    [DllImport("user32.dll")]
    //    [return: MarshalAs(UnmanagedType.Bool)]
    //    public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

    //    [DllImport("gdi32.dll")]
    //    public static extern bool DeleteObject(IntPtr handle);

    //    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    //    extern static bool DestroyIcon(IntPtr handle);

    //    // Based on the article and comments here:
    //    // http://www.switchonthecode.com/tutorials/csharp-tutorial-how-to-use-custom-cursors
    //    // Note that the returned Cursor must be disposed of after use, or you'll leak memory!
    //    public static Cursor CreateCursor(Visual target, int xHotspot, int yHotspot)
    //    {
    //        // do some things to bitmap here if necessary

    //        //return new Cursor(CreateBitmapFromVisual(target));

    //        return CreateCursor(target);




    //        //IntPtr cursorPtr;
    //        //IntPtr ptr = bm.GetHicon();
    //        //IconInfo tmp = new IconInfo();
    //        //GetIconInfo(ptr, ref tmp);
    //        //tmp.xHotspot = xHotspot;
    //        //tmp.yHotspot = yHotspot;
    //        //tmp.fIcon = false;
    //        //cursorPtr = CreateIconIndirect(ref tmp);

    //        //if (tmp.hbmColor != IntPtr.Zero) DeleteObject(tmp.hbmColor);
    //        //if (tmp.hbmMask != IntPtr.Zero) DeleteObject(tmp.hbmMask);
    //        //if (ptr != IntPtr.Zero) DestroyIcon(ptr);

    //        //return new Cursor(ptr);
    //    }

    //    //public static Bitmap AsBitmap(Control c)
    //    //{
    //    //    Bitmap bm = new Bitmap((int)c.Width, (int)c.Height);
    //    //    c.DrawToBitmap(bm, new Rectangle(0, 0, c.Width, c.Height));
    //    //    return bm;
    //    //}

    //    //public static MemoryStream ToMemoryStream(Bitmap b)
    //    //{
    //    //    MemoryStream ms = new MemoryStream();
    //    //    b.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
    //    //    return ms;
    //    //}


    //    public static MemoryStream CreateBitmapFromVisual(Visual target)
    //    {
    //        //if (target == null || string.IsNullOrEmpty(fileName))
    //        //{
    //        //    return;
    //        //}

    //        Rect bounds = VisualTreeHelper.GetDescendantBounds(target);

    //        RenderTargetBitmap renderTarget = new RenderTargetBitmap((Int32)bounds.Width, (Int32)bounds.Height, 96, 96, PixelFormats.Pbgra32);

    //        DrawingVisual visual = new DrawingVisual();

    //        using (DrawingContext context = visual.RenderOpen())
    //        {
    //            VisualBrush visualBrush = new VisualBrush(target);
    //            context.DrawRectangle(visualBrush, null, new Rect(new System.Windows.Point(), bounds.Size));
    //        }

    //        renderTarget.Render(visual);
    //        PngBitmapEncoder bitmapEncoder = new PngBitmapEncoder();
    //        bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTarget));

    //        MemoryStream ms = new MemoryStream();
    //        bitmapEncoder.Save(ms);
    //        return ms;

    //        //using (Stream stm = File.Create(fileName))
    //        //{
    //        //    bitmapEncoder.Save(stm);
    //        //}
    //    }

    //    private static Cursor CreateCursor(Visual target)
    //    {
    //        using (var ms1 = new MemoryStream())
    //        {

    //            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);

    //            RenderTargetBitmap renderTarget = new RenderTargetBitmap((Int32)bounds.Width, (Int32)bounds.Height, 96, 96, PixelFormats.Pbgra32);

    //            DrawingVisual visual = new DrawingVisual();

    //            using (DrawingContext context = visual.RenderOpen())
    //            {
    //                VisualBrush visualBrush = new VisualBrush(target);
    //                context.DrawRectangle(visualBrush, null, new Rect(new System.Windows.Point(), bounds.Size));
    //            }

    //            renderTarget.Render(visual);
    //            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
    //            pngEncoder.Frames.Add(BitmapFrame.Create(renderTarget));






    //            //var pngEncoder = new PngBitmapEncoder();
    //            //pngEncoder.Frames.Add(BitmapFrame.Create(bitmapSource));
    //            //pngEncoder.Save(ms1);

    //            var pngBytes = ms1.ToArray();
    //            var size = pngBytes.GetLength(0);

    //            using (var ms = new MemoryStream())
    //            {
    //                //Reserved must be zero; 2 bytes
    //                ms.Write(BitConverter.GetBytes((short)0), 0, 2);

    //                //image type 1 = ico 2 = cur; 2 bytes
    //                ms.Write(BitConverter.GetBytes((short)2), 0, 2);

    //                //number of images; 2 bytes
    //                ms.Write(BitConverter.GetBytes((short)1), 0, 2);

    //                //image width in pixels
    //                ms.WriteByte(32);

    //                //image height in pixels
    //                ms.WriteByte(32);

    //                //Number of Colors in the color palette. Should be 0 if the image doesn't use a color palette
    //                ms.WriteByte(0);

    //                //reserved must be 0
    //                ms.WriteByte(0);

    //                //2 bytes. In CUR format: Specifies the horizontal coordinates of the hotspot in number of pixels from the left.
    //                ms.Write(BitConverter.GetBytes((short)bounds.Width), 0, 2);
    //                //2 bytes. In CUR format: Specifies the vertical coordinates of the hotspot in number of pixels from the top.
    //                ms.Write(BitConverter.GetBytes((short)bounds.Height), 0, 2);

    //                //Specifies the size of the image's data in bytes
    //                ms.Write(BitConverter.GetBytes(size), 0, 4);

    //                //Specifies the offset of BMP or PNG data from the beginning of the ICO/CUR file
    //                ms.Write(BitConverter.GetBytes(22), 0, 4);

    //                ms.Write(pngBytes, 0, size); //write the png data.
    //                ms.Seek(0, SeekOrigin.Begin);
    //                return new Cursor(ms);
    //            }
    //        }
    //    }

    //}











    class ItemDragDropBehavior : Behavior<UIElement>
    {
        readonly TranslateTransform transform = new TranslateTransform();
        DataObject buffer;
        System.Windows.Point elementStartPosition;
        System.Windows.Point mouseStartPosition;
        System.Windows.Point positionInBlock;
        BasicVectorItemVM vmOfOriginalElement;
        BasicVectorItemVM vmOfTmpElement;
        UserControl tmpItem;

        int tmpCounter;

        Cursor curentCursor;

        private Window _dragdropWindow = null;


        bool CopyOperation
        {
            get
            {
                return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) || vmOfOriginalElement.StoredInCatalog;
            }
        }

        protected override void OnAttached()
        {
            try
            {
                // Access to VM
                vmOfOriginalElement = (BasicVectorItemVM)((UserControl)AssociatedObject).Resources["ViewModel"];
            }
            catch
            {
                // See what happens, in error case. Can we just ignore it?
                return;
            }

            base.OnAttached();

            this.AssociatedObject.MouseEnter += new MouseEventHandler(AssociatedObject_MouseEnter);
            this.AssociatedObject.MouseLeave += new MouseEventHandler(AssociatedObject_MouseLeave);
            this.AssociatedObject.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(AssociatedObject_MouseLeftButtonDown);
            this.AssociatedObject.GiveFeedback += new GiveFeedbackEventHandler(AssociatedObject_GiveFeedback);
            this.AssociatedObject.MouseLeftButtonUp += new MouseButtonEventHandler(AssociatedObject_MouseLeftButtonUp);
            //this.AssociatedObject.MouseMove += new MouseEventHandler(AssociatedObject_MouseMove);




        }


        public void StartDragging(Control c, System.Windows.Point currentMousrPoint)
        {
            //Dragged = c;
            //DisposeOldCursors();
            //Bitmap bm = CursorUtil.AsBitmap(c);

            

            //curentCursor = CursorUtil.CreateCursor(c, (int)currentMousrPoint.X, (int)currentMousrPoint.Y);





            //DragCursorMove = CursorUtil.CreateCursor((Bitmap)bm.Clone(), DragStart.X, DragStart.Y);
            //DragCursorLink = CursorUtil.CreateCursor((Bitmap)bm.Clone(), DragStart.X, DragStart.Y);
            //DragCursorCopy = CursorUtil.CreateCursor(CursorUtil.AddCopySymbol(bm), DragStart.X, DragStart.Y);
            //DragCursorNo = CursorUtil.CreateCursor(CursorUtil.AddNoSymbol(bm), DragStart.X, DragStart.Y);
            //Debug.WriteLine("Starting drag");
        }

        // This gets called once when we move over a new control,
        // or continuously if that control supports dropping.
        public void UpdateCursor(object sender, GiveFeedbackEventArgs fea)
        {
            //Debug.WriteLine(MainForm.MousePosition);
            fea.UseDefaultCursors = false;
            //Debug.WriteLine("effect = " + fea.Effect);


            Mouse.OverrideCursor = curentCursor;


            //if (fea.Effect == DragDropEffects.Move)
            //{
            //    Cursor.Current = DragCursorMove;
            //    Mouse.OverrideCursor = 

            //}
            //else if (fea.Effect == DragDropEffects.Copy)
            //{
            //    Cursor.Current = DragCursorCopy;

            //}
            //else if (fea.Effect == DragDropEffects.None)
            //{
            //    Cursor.Current = DragCursorNo;

            //}
            //else if (fea.Effect == DragDropEffects.Link)
            //{
            //    Cursor.Current = DragCursorLink;

            //}
            //else
            //{
            //    Cursor.Current = DragCursorMove;
            //}
        }


        void AssociatedObject_MouseEnter(object sender, MouseEventArgs e)
        {
            // show some thin border around of element on Enter and hide in Leave
        }

        

        void AssociatedObject_MouseLeave(object sender, MouseEventArgs e)
        {
            // show some thin border around of element on Enter and hide in Leave

            //if (isMouseClicked)
            //{
            //    //set the item's DataContext as the data to be transferred
            //    IDragable dragObject = this.AssociatedObject.DataContext as IDragable;
            //    if (dragObject != null)
            //    {
            //        DataObject data = new DataObject();
            //        data.SetData(dragObject.DataType, this.AssociatedObject.DataContext);
            //        System.Windows.DragDrop.DoDragDrop(this.AssociatedObject, data, DragDropEffects.Move);
            //    }
            //}
            //isMouseClicked = false;
        }


        void AssociatedObject_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            AssociatedObject.RenderTransform = transform;
            elementStartPosition.X = transform.X;
            elementStartPosition.Y = transform.Y;

            var parent = VisualTreeHelper.GetParent(AssociatedObject) as UIElement;
            mouseStartPosition = e.GetPosition(parent);
            AssociatedObject.CaptureMouse();














            //var parent = VisualTreeHelper.GetParent(AssociatedObject) as UIElement;
            //mouseStartPosition = e.GetPosition(parent)
            //StartDragging((UserControl)AssociatedObject, e.GetPosition(parent));









            //AssociatedObject.Effect = new DropShadowEffect
            //{
            //    Color = new System.Windows.Media.Color { A = 50, R = 0, G = 0, B = 0 },
            //    Direction = 320,
            //    ShadowDepth = 0,
            //    Opacity = .75,
            //};


            // create the visual feedback drag and drop item
            //CreateDragDropWindow(AssociatedObject);
            DragDrop.DoDragDrop(AssociatedObject, AssociatedObject, DragDropEffects.Move);







            //AssociatedObject.RenderTransform = transform;
            //elementStartPosition.X = transform.X;
            //elementStartPosition.Y = transform.Y;

            //var parent = VisualTreeHelper.GetParent(AssociatedObject) as UIElement;
            //mouseStartPosition = e.GetPosition(parent);
            //AssociatedObject.CaptureMouse();










            // Clone item
            //tmpItem = XamlReader.Parse(XamlWriter.Save((UserControl)AssociatedObject)) as UserControl;
            //vmOfTmpElement = (BasicVectorItemVM)tmpItem.Resources["ViewModel"];
            //vmOfTmpElement.CopySettingsFromAnotherVM(vmOfOriginalElement);

            // Put to the same place
            // HOW????

        }

        private void AssociatedObject_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            // update the position of the visual feedback item
            //tmpCounter++;

            //if (tmpCounter > 100)
            //{
            //    MessageBox.Show(tmpCounter.ToString());
            //}

            //if (AssociatedObject.IsMouseCaptured)
            //{
                var container = VisualTreeHelper.GetParent(AssociatedObject) as UIElement;

                if (container == null)
                    return;


            //Mouse.GetPosition(Application.Current.MainWindow);


            // get the position within the container
            //var mousePosition = Mouse.GetPosition(container);
            System.Windows.Point point = new System.Windows.Point();

            Application.Current.MainWindow.PointToScreen(point);

            var mousePosition = Mouse.GetPosition(Application.Current.MainWindow);


            System.Windows.Point pointToWindow = Mouse.GetPosition(AssociatedObject);
            //System.Windows.Point pointToScreen = PointToScreen(pointToWindow);



            // move the usercontrol.
            AssociatedObject.RenderTransform = new TranslateTransform(mousePosition.X - positionInBlock.X, mousePosition.Y - positionInBlock.Y);

            //DragDrop.DoDragDrop(AssociatedObject, "ololo", DragDropEffects.Copy);
            //}

            //OK, EVERY MOUSE MOVE WILL TRIGGER THIS EVENT
            //BUT HOW TO GET NORMAL MOUSE COORDINATES? LOOKS LIKE THERE IS NO NORMAL WAY AND YOZ HAVE TO FIND IT OUT
            // STANDART APPROACH WILL RETURN SOME CRAZY VALUES AND I DUNNO HOW TO HANDLE IT
            // MAYBE WE HAVE TO USE GLOBAL COORDINATE SYSTEM, BUT IT WILL CAUSE A LOT NEW PROBLEMS LIKE DPI E.G....










            //UpdateCursor(sender, e);



            //Win32Point w32Mouse = new Win32Point();
            //GetCursorPos(ref w32Mouse);

            //this._dragdropWindow.Left = w32Mouse.X;
            //this._dragdropWindow.Top = w32Mouse.Y;
        }

        private void CreateDragDropWindow(Visual dragElement)
        {
            this._dragdropWindow = new Window();
            _dragdropWindow.WindowStyle = WindowStyle.None;
            _dragdropWindow.AllowsTransparency = true;
            _dragdropWindow.AllowDrop = false;
            _dragdropWindow.Background = null;
            _dragdropWindow.IsHitTestVisible = false;
            _dragdropWindow.SizeToContent = SizeToContent.WidthAndHeight;
            _dragdropWindow.Topmost = true;
            _dragdropWindow.ShowInTaskbar = false;

            System.Windows.Shapes.Rectangle r = new System.Windows.Shapes.Rectangle();
            r.Width = ((FrameworkElement)dragElement).ActualWidth;
            r.Height = ((FrameworkElement)dragElement).ActualHeight;
            r.Fill = new VisualBrush(dragElement);
            this._dragdropWindow.Content = r;


            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);


            this._dragdropWindow.Left = w32Mouse.X;
            this._dragdropWindow.Top = w32Mouse.Y;
            this._dragdropWindow.Show();
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };


        void AssociatedObject_MouseMove(object sender, MouseEventArgs e)
        {
            if (AssociatedObject.IsMouseCaptured)
            {
                var container = VisualTreeHelper.GetParent(AssociatedObject) as UIElement;

                if (container == null)
                    return;

                // get the position within the container
                var mousePosition = e.GetPosition(container);


                // move the usercontrol.
                AssociatedObject.RenderTransform = new TranslateTransform(mousePosition.X - positionInBlock.X, mousePosition.Y - positionInBlock.Y);

                DragDrop.DoDragDrop(AssociatedObject, "ololo", DragDropEffects.Copy);
            }

        }



        void AssociatedObject_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            




            //AssociatedObject.ReleaseMouseCapture();

            //var container = VisualTreeHelper.GetParent(AssociatedObject) as UIElement;

            //if (container == null)
            //    return;

            //if (!container.AllowDrop)
            //{
            //    // return item on start position
            //    if (!CopyOperation)
            //    {
            //        // We can be sure that this opertaon happens only on Canvas element, restore canvas properties
            //        transform.X = elementStartPosition.X;
            //        transform.Y = elementStartPosition.Y;
            //    }
            //    else
            //    {
            //        // If there is copy, we have to destroy tmp element

            //    }
            //}

            //// get the position within the container
            //var mousePosition = e.GetPosition(container);

            //if ()


            //YOU CAN NOT MOVE IT TO ANOTHER ELEMENT, IT WILL BE ALWAYS ON PARENT ELEMENT
            //SO USE DROP EVENTS FOR SOME PART OF THIS JOB LIKE YOU CAN MOVE THE ITEM AND ON RELEASING CALL DROPPING EVENT OR SO...
            //AND WE NO NEED CHECK PARENT IN THIS CASE EVERY TIME



            


        }














































        //protected override void OnAttached()
        //{
        //    Window parent = Application.Current.MainWindow;
        //    AssociatedObject.RenderTransform = _transform;

        //    AssociatedObject.MouseLeftButtonDown += (sender, e) =>
        //    {
        //        _mouseStartPosition = e.GetPosition(parent);
        //        AssociatedObject.CaptureMouse();
        //    };

        //    AssociatedObject.MouseLeftButtonUp += (sender, e) =>
        //    {
        //        AssociatedObject.ReleaseMouseCapture();
        //        _elementStartPosition.X = _transform.X;
        //        _elementStartPosition.Y = _transform.Y;

        //    };

        //    AssociatedObject.MouseMove += (sender, e) =>
        //    {
        //        var mousePos = e.GetPosition(parent);
        //        var diff = (mousePos - _mouseStartPosition);
        //        if (!AssociatedObject.IsMouseCaptured) return;
        //        _transform.X = _elementStartPosition.X + diff.X;
        //        _transform.Y = _elementStartPosition.Y + diff.Y;
        //    };
        //}

        //private readonly TranslateTransform _transform = new TranslateTransform();
        //private Point _elementStartPosition;
        //private Point _mouseStartPosition;


































        //public readonly TranslateTransform Transform = new TranslateTransform();
        //private Point _elementStartPosition2;
        //private Point _mouseStartPosition2;
        //private static ItemDragDropBehavior _instance = new ItemDragDropBehavior();
        //public static ItemDragDropBehavior Instance
        //{
        //    get { return _instance; }
        //    set { _instance = value; }
        //}

        //public static bool GetDrag(DependencyObject obj)
        //{
        //    return (bool)obj.GetValue(IsDragProperty);
        //}

        //public static void SetDrag(DependencyObject obj, bool value)
        //{
        //    obj.SetValue(IsDragProperty, value);
        //}

        //public static readonly DependencyProperty IsDragProperty =
        //  DependencyProperty.RegisterAttached("Drag",
        //  typeof(bool), typeof(ItemDragDropBehavior),
        //  new PropertyMetadata(false, OnDragChanged));

        //private static void OnDragChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    // ignoring error checking
        //    var element = (UIElement)sender;
        //    var isDrag = (bool)(e.NewValue);

        //    Instance = new ItemDragDropBehavior();
        //    ((UIElement)sender).RenderTransform = Instance.Transform;

        //    if (isDrag)
        //    {
        //        element.MouseLeftButtonDown += Instance.ElementOnMouseLeftButtonDown;
        //        element.MouseLeftButtonUp += Instance.ElementOnMouseLeftButtonUp;
        //        element.MouseMove += Instance.ElementOnMouseMove;
        //    }
        //    else
        //    {
        //        element.MouseLeftButtonDown -= Instance.ElementOnMouseLeftButtonDown;
        //        element.MouseLeftButtonUp -= Instance.ElementOnMouseLeftButtonUp;
        //        element.MouseMove -= Instance.ElementOnMouseMove;
        //    }
        //}

        //private void ElementOnMouseLeftButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        //{
        //    var parent = Application.Current.MainWindow;
        //    _mouseStartPosition2 = mouseButtonEventArgs.GetPosition(parent);
        //    ((UIElement)sender).CaptureMouse();
        //}

        //private void ElementOnMouseLeftButtonUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        //{
        //    ((UIElement)sender).ReleaseMouseCapture();
        //    _elementStartPosition2.X = Transform.X;
        //    _elementStartPosition2.Y = Transform.Y;
        //}

        //private void ElementOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        //{
        //    var parent = Application.Current.MainWindow;
        //    var mousePos = mouseEventArgs.GetPosition(parent);
        //    var diff = (mousePos - _mouseStartPosition2);
        //    if (!((UIElement)sender).IsMouseCaptured) return;
        //    Transform.X = _elementStartPosition2.X + diff.X;
        //    Transform.Y = _elementStartPosition2.Y + diff.Y;
        //}


    }
}
