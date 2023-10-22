using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpandScadaEditor.ScreenEditor.Items;

namespace ExpandScadaEditor.ScreenEditor
{
    /*  New concept
     *  
     *      - first action - load all elements on screen on load
     *      - every new user action:
     *          - send all elements on screen here just by reference
     *          - compare every element with previous elements collection and find a differences
     *              - 
     * 
     * 
     * 
     * */

    public class UndoRedoContainer
    {
        Stack<(List<ScreenElement> elements, UndoRedoActionType actionType)> UndoCollection = new Stack<(List<ScreenElement> elements, UndoRedoActionType actionType)>();
        Stack<(List<ScreenElement> elements, UndoRedoActionType actionType)> RedoCollection = new Stack<(List<ScreenElement> elements, UndoRedoActionType actionType)>();

        List<int> existingElementsId = new List<int>();

        public bool UndoIsPossible()
        {
            return UndoCollection.Count > 1;
        }

        public bool RedoIsPossible()
        {
            return RedoCollection.Count > 0;
        }

        public void BasicUserAction(List<ScreenElement> initElements)
        {
            List<ScreenElement> clonedElements = new List<ScreenElement>();
            foreach (var element in initElements)
            {
                var type = element.GetType();
                var newItem = (ScreenElement)Activator.CreateInstance(type, element.ElementContent);
                newItem.InitializeFromAnotherElement(element);
                clonedElements.Add(newItem);
                existingElementsId.Add(element.Id);
            }

            UndoCollection.Push((clonedElements, UndoRedoActionType.Base));
        }

        //public void NewUserAction(ScreenElement currentElement, UndoRedoActionType actionType = UndoRedoActionType.Replace)
        //{
        //    NewUserAction(new List<ScreenElement>() { currentElement }, actionType);
        //}



        public void NewUserAction(Dictionary<int, ScreenElement> currentElements, UndoRedoActionType actionType = UndoRedoActionType.Replace)
        {
            switch (actionType)
            {
                case UndoRedoActionType.Replace:
                    List<ScreenElement> changedElements = new List<ScreenElement>();
                    foreach (var currentElement in currentElements.Values)
                    {
                        // check if current element exists in basic list of elements (just list of ID)
                        // if not - act like it was added - just copy
                        // if yes - check closest element from stack which has this ID and compare with it. 
                        //      - if it is different - save new copy, if same - ignore

                        // find closest copy of this element in undo stack
                        ScreenElement elementInPastAction = null;
                        foreach (var undoAction in UndoCollection)
                        {
                            elementInPastAction = undoAction.elements.Find(x => x.Id == currentElement.Id);
                            if (elementInPastAction is not null)
                            {
                                break;
                            }
                        }

                        if (elementInPastAction is null)
                        {
                            throw new InvalidOperationException($"Changed element with id {currentElement.Id} not found in changing history");
                        }

                        if (currentElement != elementInPastAction)
                        {
                            var type = currentElement.GetType();
                            var newItem = (ScreenElement)Activator.CreateInstance(type, currentElement.ElementContent);
                            newItem.InitializeFromAnotherElement(currentElement);
                            changedElements.Add(newItem);
                        }
                    }

                    if (changedElements.Count == 0)
                    {
                        //throw new InvalidOperationException($"Conflict found: User action \"Changed elements\" was occured, but no element was really changed");

                        // Basically it is better to throw an exception, but there is cases when action made without changes. So - just ignore and do nothing
                        // But later fix it of cource and throw an exceotion
                        return;
                    }
                    UndoCollection.Push((changedElements, actionType));
                    break;
                case UndoRedoActionType.Create:
                    List<ScreenElement> createdElements = new List<ScreenElement>();
                    foreach (var currentElement in currentElements.Values)
                    {
                        if (!existingElementsId.Contains(currentElement.Id))
                        {
                            var type = currentElement.GetType();
                            var newItem = (ScreenElement)Activator.CreateInstance(type, currentElement.ElementContent);
                            newItem.InitializeFromAnotherElement(currentElement);
                            createdElements.Add(newItem);
                            existingElementsId.Add(newItem.Id);
                        }
                    }

                    if (createdElements.Count == 0)
                    {
                        throw new InvalidOperationException($"Conflict found: User action \"Created elements\" was occured, but no element was really added");
                    }
                    UndoCollection.Push((createdElements, actionType));
                    break;
                case UndoRedoActionType.Delete:
                    List<ScreenElement> deletedElements = new List<ScreenElement>();

                    foreach (var existingID in existingElementsId)
                    {
                        var elementTmp = currentElements.Values.FirstOrDefault(x => x.Id == existingID);
                        if (elementTmp is null)
                        {
                            // if element was deleted - find last change in this element by id and store it - to restore it after
                            // find closest copy of this element in undo stack
                            ScreenElement elementInPastAction = null;
                            foreach (var undoAction in UndoCollection)
                            {
                                elementInPastAction = undoAction.elements.Find(x => x.Id == existingID);
                                if (elementInPastAction is not null)
                                {
                                    break;
                                }
                            }

                            if (elementInPastAction is null)
                            {
                                throw new InvalidOperationException($"Changed element with id {existingID} not found in changing history");
                            }

                            var type = elementInPastAction.GetType();
                            var newItem = (ScreenElement)Activator.CreateInstance(type, elementInPastAction.ElementContent);
                            newItem.InitializeFromAnotherElement(elementInPastAction);
                            deletedElements.Add(newItem);
                        }
                    }

                    if (deletedElements.Count == 0)
                    {
                        throw new InvalidOperationException($"Conflict found: User action \"Deleted elements\" was occured, but no element was really deleted");
                    }

                    deletedElements.ForEach(x => existingElementsId.Remove(x.Id));

                    UndoCollection.Push((deletedElements, actionType));
                    break;
            }


            RedoCollection.Clear();

        }



        public (List<ScreenElement> elements, UndoRedoActionType actionType) Undo()
        {
            // take last action and move it to the redo
            // take pre action and return it without removing
            (List<ScreenElement> elements, UndoRedoActionType actionType) elementsToReturn;

            switch (UndoCollection.Peek().actionType)
            {
                case UndoRedoActionType.Base:
                    elementsToReturn = UndoCollection.Peek();
                    break;
                case UndoRedoActionType.Create:
                case UndoRedoActionType.Delete:
                    // take last changre and move the same change to redo list
                    RedoCollection.Push(UndoCollection.Peek());
                    elementsToReturn = UndoCollection.Pop();
                    break;
                case UndoRedoActionType.Replace:
                    var elementsInNewState = UndoCollection.Peek();
                    RedoCollection.Push(UndoCollection.Pop());
                    // we have to find last state of changed elements. it could be any pre-action. have to find it
                    elementsToReturn = (new List<ScreenElement>(), elementsInNewState.actionType);
                    foreach (var element in elementsInNewState.elements)
                    {
                        ScreenElement elementInPastAction = null;
                        foreach (var undoAction in UndoCollection)
                        {
                            elementInPastAction = undoAction.elements.Find(x => x.Id == element.Id);
                            if (elementInPastAction is not null)
                            {
                                break;
                            }
                        }

                        if (elementInPastAction is null)
                        {
                            throw new InvalidOperationException($"Changed element with id {element.Id} not found in changing UNDO history");
                        }

                        elementsToReturn.elements.Add(elementInPastAction);
                    }

                    //elementsToReturn = UndoCollection.Peek();
                    break;
                default:
                    elementsToReturn = (null, UndoRedoActionType.Replace);
                    break;
            }

            List<ScreenElement> clonedElements = new List<ScreenElement>();
            foreach (var element in elementsToReturn.elements)
            {
                var type = element.GetType();
                var newItem = (ScreenElement)Activator.CreateInstance(type, element.ElementContent);
                newItem.InitializeFromAnotherElement(element);
                clonedElements.Add(newItem);
            }

            // if actionType is create/delete - we have to update ID list of current elements
            switch (elementsToReturn.actionType)
            {
                case UndoRedoActionType.Create:
                    // If this was a create action and we have to UNDO it - then we have to act like delete action
                    clonedElements.ForEach(x => existingElementsId.Remove(x.Id));
                    break;
                case UndoRedoActionType.Delete:
                    // If this was a delete action and we have to UNDO it - then we have to act like create action
                    clonedElements.ForEach(x => existingElementsId.Add(x.Id));
                    break;
            }

            return (clonedElements, elementsToReturn.actionType);
        }


        public (List<ScreenElement> elements, UndoRedoActionType actionType) Redo()
        {
            (List<ScreenElement> elements, UndoRedoActionType actionType) elementsToReturn;

            switch (RedoCollection.Peek().actionType)
            {
                case UndoRedoActionType.Create:
                case UndoRedoActionType.Delete:
                    // take last changre and move the same change to redo list
                    UndoCollection.Push(RedoCollection.Peek());
                    elementsToReturn = RedoCollection.Pop();
                    break;
                case UndoRedoActionType.Replace:

                    // in undo action we dont care about current UNDO action, looking only for pre-state
                    // in redo action try to just take as is first...

                    elementsToReturn = RedoCollection.Peek();
                    UndoCollection.Push(RedoCollection.Pop());



                    //var elementsInNewState = RedoCollection.Peek();
                    //UndoCollection.Push(RedoCollection.Pop());

                    //// we have to find last state of changed elements. it could be any pre-action. have to find it
                    //elementsToReturn = (new List<ScreenElement>(), elementsInNewState.actionType);
                    //foreach (var element in elementsInNewState.elements)
                    //{
                    //    ScreenElement elementInPastAction = null;
                    //    foreach (var redoAction in RedoCollection)
                    //    {
                    //        elementInPastAction = redoAction.elements.Find(x => x.Id == element.Id);
                    //        if (elementInPastAction is not null)
                    //        {
                    //            break;
                    //        }
                    //    }

                    //    if (elementInPastAction is null)
                    //    {
                    //        throw new InvalidOperationException($"Changed element with id {element.Id} not found in changing REDO history");
                    //    }

                    //    elementsToReturn.elements.Add(elementInPastAction);
                    //}

                    //elementsToReturn = UndoCollection.Peek();
                    break;
                default:
                    elementsToReturn = (null, UndoRedoActionType.Replace);
                    break;
            }



            List<ScreenElement> clonedElements = new List<ScreenElement>();
            foreach (var element in elementsToReturn.elements)
            {
                var type = element.GetType();
                var newItem = (ScreenElement)Activator.CreateInstance(type, element.ElementContent);
                newItem.InitializeFromAnotherElement(element);
                clonedElements.Add(newItem);
            }

            // if actionType is create/delete - we have to update ID list of current elements
            switch (elementsToReturn.actionType)
            {
                case UndoRedoActionType.Create:
                    // If this was a create action and we have to REDO it - then we have to act like create action
                    clonedElements.ForEach(x => existingElementsId.Add(x.Id));
                    break;
                case UndoRedoActionType.Delete:
                    // If this was a delete action and we have to REDO it - then we have to act like delete action
                    clonedElements.ForEach(x => existingElementsId.Remove(x.Id));
                    break;
            }

            return (clonedElements, elementsToReturn.actionType);
















        }



    }



    //public class UndoRedoContainer
    //{
    //    Stack<(List<ScreenElement> elements, bool werePasted)> UndoCollection = new Stack<(List<ScreenElement> elements, bool werePasted)>();
    //    Stack<(List<ScreenElement> elements, bool werePasted)> RedoCollection = new Stack<(List<ScreenElement> elements, bool werePasted)>();

    //    public void NewUserAction(List<ScreenElement> changedElements, bool onPaste = false)
    //    {
    //        List<ScreenElement> clonedElements = new List<ScreenElement>();
    //        foreach (var element in changedElements)
    //        {
    //            var type = element.GetType();
    //            var newItem = (ScreenElement)Activator.CreateInstance(type);
    //            newItem.InitializeFromAnotherElement(element);
    //            //newItem.ToDelete = onPaste;
    //            clonedElements.Add(newItem);
    //        }

    //        UndoCollection.Push((clonedElements, onPaste));
    //        RedoCollection.Clear();

    //        //changedElements.ForEach(x => clonedElements.Add((ScreenElement)x.Clone()));
    //        //UndoCollection.Push(clonedElements);
    //        //RedoCollection.Clear();
    //    }

    //    public void NewUserAction(ScreenElement changedElement, bool onPaste = false)
    //    {
    //        var type = changedElement.GetType();
    //        var newItem = (ScreenElement)Activator.CreateInstance(type);
    //        newItem.InitializeFromAnotherElement(changedElement);
    //        //newItem.ToDelete = onPaste;
    //        UndoCollection.Push((new List<ScreenElement>() { newItem }, onPaste));
    //        RedoCollection.Clear();

    //        //UndoCollection.Push(new List<ScreenElement>() { (ScreenElement)changedElement.Clone() });
    //        //RedoCollection.Clear();
    //    }

    //    public (List<ScreenElement> elements, bool werePasted) Undo()
    //    {
    //        // if last action - were pasted, then we have to move this action to redo and same action out
    //        var elementsFromLastAction = UndoCollection.Peek();
    //        (List<ScreenElement> elements, bool werePasted) elementsToReturn;
    //        if (elementsFromLastAction.werePasted)
    //        {
    //            RedoCollection.Push(elementsFromLastAction);
    //            elementsToReturn = UndoCollection.Pop();
    //        }
    //        else
    //        {
    //            RedoCollection.Push(UndoCollection.Pop());
    //            elementsToReturn = UndoCollection.Peek();
    //        }

    //        List<ScreenElement> clonedElements = new List<ScreenElement>();
    //        foreach (var element in elementsToReturn.elements)
    //        {
    //            var type = element.GetType();
    //            var newItem = (ScreenElement)Activator.CreateInstance(type);
    //            newItem.InitializeFromAnotherElement(element);
    //            clonedElements.Add(newItem);
    //        }
    //        return (clonedElements, elementsFromLastAction.werePasted);


    //        //RedoCollection.Push(UndoCollection.Pop());

    //        //var elementsToReturn = UndoCollection.Peek();
    //        //List<ScreenElement> clonedElements = new List<ScreenElement>();
    //        //foreach (var element in elementsToReturn)
    //        //{
    //        //    var type = element.GetType();
    //        //    var newItem = (ScreenElement)Activator.CreateInstance(type);
    //        //    newItem.InitializeFromAnotherElement(element);
    //        //    clonedElements.Add(newItem);
    //        //}

    //        //return clonedElements;
    //    }

    //    public (List<ScreenElement> elements, bool werePasted) Redo()
    //    {
    //        UndoCollection.Push(RedoCollection.Peek());
    //        return RedoCollection.Pop();
    //    }

    //    public bool UndoIsPossible()
    //    {
    //        return UndoCollection.Count > 1;
    //    }

    //    public bool RedoIsPossible()
    //    {
    //        return RedoCollection.Count > 0;
    //    }
    //}
}
