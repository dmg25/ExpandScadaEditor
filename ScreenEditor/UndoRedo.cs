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
            foreach (var element in changedElements)
            {
                var type = element.GetType();
                var newItem = (ScreenElement)Activator.CreateInstance(type);
                newItem.InitializeFromAnotherElement(element);
                clonedElements.Add(newItem);
            }

            UndoCollection.Push(clonedElements);
            RedoCollection.Clear();

            //changedElements.ForEach(x => clonedElements.Add((ScreenElement)x.Clone()));
            //UndoCollection.Push(clonedElements);
            //RedoCollection.Clear();
        }

        public void NewUserAction(ScreenElement changedElement)
        {
            var type = changedElement.GetType();
            var newItem = (ScreenElement)Activator.CreateInstance(type);
            newItem.InitializeFromAnotherElement(changedElement);
            UndoCollection.Push(new List<ScreenElement>() { newItem });
            RedoCollection.Clear();

            //UndoCollection.Push(new List<ScreenElement>() { (ScreenElement)changedElement.Clone() });
            //RedoCollection.Clear();
        }

        public List<ScreenElement> Undo()
        {
            RedoCollection.Push(UndoCollection.Pop());

            var elementsToReturn = UndoCollection.Peek();
            List<ScreenElement> clonedElements = new List<ScreenElement>();
            foreach (var element in elementsToReturn)
            {
                var type = element.GetType();
                var newItem = (ScreenElement)Activator.CreateInstance(type);
                newItem.InitializeFromAnotherElement(element);
                clonedElements.Add(newItem);
            }

            return clonedElements;
        }

        public List<ScreenElement> Redo()
        {
            UndoCollection.Push(RedoCollection.Pop());
            return RedoCollection.Peek();
        }

        public bool UndoIsPossible()
        {
            return UndoCollection.Count > 1;
        }

        public bool RedoIsPossible()
        {
            return RedoCollection.Count > 0;
        }
    }
}
