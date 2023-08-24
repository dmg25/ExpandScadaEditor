using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpandScadaEditor.ScreenEditor
{
    public enum UndoRedoActionType
    {
        Base,
        Replace,
        Create,
        Delete
    }

    public enum MouseMovingMode
    {
        None,
        MoveSelectedElements,
        Selecting,
        CopyDuringMoving
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
        RotateToRight,
        RotateToLeft
    }
}
