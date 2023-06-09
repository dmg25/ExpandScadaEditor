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
        Stack<(List<ScreenElement> elements, bool werePasted)> UndoCollection = new Stack<(List<ScreenElement> elements, bool werePasted)>();
        Stack<(List<ScreenElement> elements, bool werePasted)> RedoCollection = new Stack<(List<ScreenElement> elements, bool werePasted)>();

        public void NewUserAction(List<ScreenElement> changedElements, bool onPaste = false)
        {
            List<ScreenElement> clonedElements = new List<ScreenElement>();
            foreach (var element in changedElements)
            {
                var type = element.GetType();
                var newItem = (ScreenElement)Activator.CreateInstance(type);
                newItem.InitializeFromAnotherElement(element);
                //newItem.ToDelete = onPaste;
                clonedElements.Add(newItem);
            }

            UndoCollection.Push((clonedElements, onPaste));
            RedoCollection.Clear();

            //changedElements.ForEach(x => clonedElements.Add((ScreenElement)x.Clone()));
            //UndoCollection.Push(clonedElements);
            //RedoCollection.Clear();
        }

        public void NewUserAction(ScreenElement changedElement, bool onPaste = false)
        {
            var type = changedElement.GetType();
            var newItem = (ScreenElement)Activator.CreateInstance(type);
            newItem.InitializeFromAnotherElement(changedElement);
            //newItem.ToDelete = onPaste;
            UndoCollection.Push((new List<ScreenElement>() { newItem }, onPaste));
            RedoCollection.Clear();

            //UndoCollection.Push(new List<ScreenElement>() { (ScreenElement)changedElement.Clone() });
            //RedoCollection.Clear();
        }

        public (List<ScreenElement> elements, bool werePasted) Undo()
        {
            // if last action - were pasted, then we have to move this action to redo and same action out
            var elementsFromLastAction = UndoCollection.Peek();
            (List<ScreenElement> elements, bool werePasted) elementsToReturn;
            if (elementsFromLastAction.werePasted)
            {
                RedoCollection.Push(elementsFromLastAction);
                elementsToReturn = UndoCollection.Pop();
            }
            else
            {
                RedoCollection.Push(UndoCollection.Pop());
                elementsToReturn = UndoCollection.Peek();
            }

            List<ScreenElement> clonedElements = new List<ScreenElement>();
            foreach (var element in elementsToReturn.elements)
            {
                var type = element.GetType();
                var newItem = (ScreenElement)Activator.CreateInstance(type);
                newItem.InitializeFromAnotherElement(element);
                clonedElements.Add(newItem);
            }
            return (clonedElements, elementsFromLastAction.werePasted);


            //RedoCollection.Push(UndoCollection.Pop());

            //var elementsToReturn = UndoCollection.Peek();
            //List<ScreenElement> clonedElements = new List<ScreenElement>();
            //foreach (var element in elementsToReturn)
            //{
            //    var type = element.GetType();
            //    var newItem = (ScreenElement)Activator.CreateInstance(type);
            //    newItem.InitializeFromAnotherElement(element);
            //    clonedElements.Add(newItem);
            //}

            //return clonedElements;
        }

        public (List<ScreenElement> elements, bool werePasted) Redo()
        {
            UndoCollection.Push(RedoCollection.Peek());
            return RedoCollection.Pop();
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
