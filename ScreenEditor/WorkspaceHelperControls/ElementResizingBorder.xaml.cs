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

namespace ExpandScadaEditor.ScreenEditor.WorkspaceHelperControls
{
    /// <summary>
    /// Логика взаимодействия для ElementResizingBorder.xaml
    /// </summary>
    public partial class ElementResizingBorder : UserControl
    {
        public ElementResizingBorder()
        {
            InitializeComponent();
            SizeNWSE_leftUp.Cursor = Cursors.SizeNWSE;
            SizeNESW_rightUp.Cursor = Cursors.SizeNESW;
            SizeNESW_leftDown.Cursor = Cursors.SizeNESW;
            SizeNWSE_rightDown.Cursor = Cursors.SizeNWSE;

            SizeLeft.Cursor = Cursors.SizeWE;
            SizeRight.Cursor = Cursors.SizeWE;
            SizeTop.Cursor = Cursors.SizeNS;
            SizeBottom.Cursor = Cursors.SizeNS;
        }
    }
}
