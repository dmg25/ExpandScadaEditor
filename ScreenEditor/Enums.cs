using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpandScadaEditor.ScreenEditor
{
    public enum MouseMovingMode
    {
        None,
        MoveSelectedElements,
        Selecting,

    }

    public enum MouseSelectingDirection
    {
        RightDown,
        LeftDown,
        LeftUp,
        RightUp
    }

    public enum ResizingType
    {
        ChangeSize,
        ChangeCoordinates,
        RotateToRight,
        RotateToLeft
    }
}
