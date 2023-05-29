using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpandScadaEditor.ScreenEditor.Items;

namespace ExpandScadaEditor.ScreenEditor
{
    public class UndoRedoContainer
    {
        Stack<List<ScreenElement>> UndoCollection = new Stack<List<ScreenElement>>();
        Stack<List<ScreenElement>> RedoCollection = new Stack<List<ScreenElement>>();

        public void NewUserAction(List<ScreenElement> changedElements)
        {
            List<ScreenElement> clonedElements = new List<ScreenElement>();
            changedElements.ForEach(x => clonedElements.Add((ScreenElement)x.Clone()));
            UndoCollection.Push(clonedElements);
            RedoCollection.Clear();
        }

        public void NewUserAction(ScreenElement changedElement)
        {
            UndoCollection.Push(new List<ScreenElement>() { (ScreenElement)changedElement.Clone() });
            RedoCollection.Clear();
        }

        public List<ScreenElement> Undo()
        {
            RedoCollection.Push(UndoCollection.Peek());
            return UndoCollection.Pop();
        }

        public List<ScreenElement> Redo()
        {
            UndoCollection.Push(RedoCollection.Peek());
            return RedoCollection.Pop();
        }

        public bool UndoIsPossible()
        {
            return UndoCollection.Count != 0;
        }

        public bool RedoIsPossible()
        {
            return RedoCollection.Count != 0;
        }
    }
}
