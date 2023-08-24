using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ExpandScadaEditor.ScreenEditor.Items;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ExpandScadaEditor.ScreenEditor
{
    public class VectorEditorVM : INotifyPropertyChanged
    {
        /*  undo/redo
         *      - Create class or service to contain user's actions
         *          - class OperationsMemory
         *          - contains Undo and Redo list/queue
         *          - item is list of ScreenElements with all actual properties
         *      - Create undo/redo methods
         *          - undo 
         *              - find all elements from undo index in workspace elements dictionary
         *                and apply all properties (or just whole object, why not)
         *              - if there is no element with this ID in dictionary - create and put on space
         *          - redo - the same
         *          - after UNdo list was used - take item and put in the redo list
         *          - if undo list was filled witn new index from workspace - clean redo list
         *      - Create 2 commands for undo and redo
         *      - think out, how to add actions to queue
         *          - just method "add user action"
         * */

        public UndoRedoContainer UndoRedo = new UndoRedoContainer();

        public event EventHandler<ScreenElementEventArgs> NewScreenElementAdded;
        public event EventHandler<ScreenElementEventArgs> ScreenElementDeleted;
        public event EventHandler SelectedElementsDeleted;
        public event EventHandler<ReplacingElementEventArgs> ScreenElementReplaced;
        public event EventHandler<ScreenElementsEventArgs> SelectTheseElements;

        internal List<ScreenElement> SelectedElements = new List<ScreenElement>();
        //internal Dictionary<int, ScreenElement> SelectedElements = new Dictionary<int, ScreenElement>();
        internal List<ScreenElement> CopyPasteBuffer = new List<ScreenElement>();

        private List<ScreenElement> items = new List<ScreenElement>();
        public List<ScreenElement> Items
        {
            get
            {
                return items;
            }
            set
            {
                items = value;
                NotifyPropertyChanged();
            }
        }

        // Only for tests for a while
        private double mouseX;
        public double MouseX
        {
            get
            {
                return mouseX;
            }
            set
            {
                mouseX = value;
                NotifyPropertyChanged();
            }
        }

        private double mouseY;
        public double MouseY
        {
            get
            {
                return mouseY;
            }
            set
            {
                mouseY = value;
                NotifyPropertyChanged();
            }
        }


        private double workSpaceHeight;
        public double WorkSpaceHeight
        {
            get
            {
                return workSpaceHeight;
            }
            set
            {
                workSpaceHeight = value;
                NotifyPropertyChanged();
            }
        }

        private double workSpaceWidth;
        public double WorkSpaceWidth
        {
            get
            {
                return workSpaceWidth;
            }
            set
            {
                workSpaceWidth = value;
                NotifyPropertyChanged();
            }
        }

        private string mouseOverElement;
        public string MouseOverElement
        {
            get
            {
                return mouseOverElement;
            }
            set
            {
                mouseOverElement = value;
                NotifyPropertyChanged();
            }
        }


        int idForNewScreenElement;
        //int IdForNewScreenElement
        //{
        //    get
        //    {
        //        return _idForNewScreenElement++;
        //    }
        //}

        public Dictionary<int, ScreenElement> ElementsOnWorkSpace { get; set; } = new Dictionary<int, ScreenElement>();



        // -------------------------------------------------------



        private Command _undo;
        public Command Undo
        {
            get
            {
                return _undo ??
                    (_undo = new Command(obj =>
                    {
                        // check which action type it was. Create/Delete must be vv.
                        var undoElements = UndoRedo.Undo();

                        switch (undoElements.actionType)
                        {
                            case UndoRedoActionType.Base:
                                foreach (var pair in ElementsOnWorkSpace)
                                {
                                    DeleteElement(pair.Value);
                                }
                                undoElements.elements.ForEach(x => AddNewScreenElement(x, true));
                                break;
                            case UndoRedoActionType.Replace:
                                undoElements.elements.ForEach(x => ReplaceExistingElement(x));
                                break;
                            case UndoRedoActionType.Create:
                                // if we UNDO creation - we have to delete
                                undoElements.elements.ForEach(x => DeleteElement(x));
                                break;
                            case UndoRedoActionType.Delete:
                                // if we UNDO deleting - we have to create
                                undoElements.elements.ForEach(x => AddNewScreenElement(x, true));
                                break;
                        }
                    },
                    obj =>
                    {
                        return UndoRedo.UndoIsPossible();
                    }));
            }
        }



        private Command _redo;
        public Command Redo
        {
            get
            {
                return _redo ??
                    (_redo = new Command(obj =>
                    {
                        var redoElements = UndoRedo.Redo();

                        switch (redoElements.actionType)
                        {
                            case UndoRedoActionType.Base:
                                foreach (var pair in ElementsOnWorkSpace)
                                {
                                    DeleteElement(pair.Value);
                                }
                                redoElements.elements.ForEach(x => AddNewScreenElement(x, true));
                                break;
                            case UndoRedoActionType.Replace:
                                redoElements.elements.ForEach(x => ReplaceExistingElement(x));
                                break;
                            case UndoRedoActionType.Create:
                                // if we REDO creation - we have to create
                                redoElements.elements.ForEach(x => AddNewScreenElement(x, true));
                                break;
                            case UndoRedoActionType.Delete:
                                // if we REDO deleting - we have to delete
                                redoElements.elements.ForEach(x => DeleteElement(x));
                                break;
                        }
                    },
                    obj =>
                    {
                        return UndoRedo.RedoIsPossible();
                    }));
            }
        }

        private Command _delete;
        public Command Delete
        {
            get
            {
                return _delete ??
                    (_delete = new Command(obj =>
                    {
                        SelectedElements.ForEach(x => ElementsOnWorkSpace.Remove(x.Id));
                        UndoRedo.NewUserAction(ElementsOnWorkSpace, UndoRedoActionType.Delete); 
                        SelectedElementsDeleted(null, new EventArgs());
                    },
                    obj =>
                    {
                        return SelectedElements.Count > 0;
                    }));
            }
        }

        private Command _copy;
        public Command Copy
        {
            get
            {
                return _copy ??
                    (_copy = new Command(obj =>
                    {
                        // clean old buffer
                        // copy all elements with all properties from selected list to buffer

                        CopyPasteBuffer.Clear();
                        CopyElementsInList(SelectedElements).ForEach(x => CopyPasteBuffer.Add(x));
                    },
                    obj =>
                    {
                        return SelectedElements.Count > 0;
                    }));
            }
        }

        private Command _paste;
        public Command Paste
        {
            get
            {
                return _paste ??
                    (_paste = new Command(obj =>
                    {
                        // put copied elements to their positions + 5px in X and Y.
                        // if even one of them is at the border and placing can not be updated - paste all of them without shifting
                        // create user action like adding new elements
                        // add new elements

                        // check if all group can be shifted by pasting
                        var ShiftingPossibility = CheckIfElementsCanBeShifted(CopyPasteBuffer);
                        if (ShiftingPossibility.XEnabled)
                        {
                            CopyPasteBuffer.ForEach(x => x.CoordX += 5);
                        }
                        if (ShiftingPossibility.YEnabled)
                        {
                            CopyPasteBuffer.ForEach(x => x.CoordY += 5);
                        }

                        List<ScreenElement> elementsForSelection = new List<ScreenElement>();
                        CopyElementsInList(CopyPasteBuffer).ForEach(x => { AddNewScreenElement(x); elementsForSelection.Add(x); });
                        SelectTheseElements(null, new ScreenElementsEventArgs() { Elements = elementsForSelection });
                        UndoRedo.NewUserAction(ElementsOnWorkSpace, UndoRedoActionType.Create);
                    },
                    obj =>
                    {
                        return CopyPasteBuffer.Count > 0;
                    }));
            }
        }

        private Command _selectAll;
        public Command SelectAll
        {
            get
            {
                return _selectAll ??
                    (_selectAll = new Command(obj =>
                    {
                        SelectTheseElements(null, new ScreenElementsEventArgs() { Elements = ElementsOnWorkSpace.Values.ToList() });
                    },
                    obj =>
                    {
                        return ElementsOnWorkSpace.Count > 0;
                    }));
            }
        }

        private Command _cut;
        public Command Cut
        {
            get
            {
                return _cut ??
                    (_cut = new Command(obj =>
                    {
                        Copy.Execute(null);
                        Delete.Execute(null);
                    },
                    obj =>
                    {
                        return SelectedElements.Count > 0;
                    }));
            }
        }



        public VectorEditorVM()
        {
            
        }

        public void Initialize()
        {
            Items.Add(new TestItem2() { CatalogMode = true, Id = -111});
            Items.Add(new TestItem2() { CatalogMode = true, Id = -222 });

            // Create/Load elements VM must be created automatically for each
            // TODO move it to loading process or smth
            AddNewScreenElement(new TestItem2() { CoordX = 10, CoordY = 20, Name = "first"});
            AddNewScreenElement(new TestItem2() { CoordX = 100, CoordY = 100, Name = "second" });
            AddNewScreenElement(new TestItem2() { CoordX = 100, CoordY = 200, Name = "third"});










            //MyCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
        }

        public void AddNewScreenElement(ScreenElement element, bool useOldId = false)
        {
            if (!useOldId)
            {
                int newId = idForNewScreenElement++;
                element.Id = newId;

                if (string.IsNullOrWhiteSpace(element.Name) || string.IsNullOrEmpty(element.Name))
                {
                    element.Name = $"element_{element.Id}";
                }
                ElementsOnWorkSpace.Add(newId, element);
            }
            else
            {
                ElementsOnWorkSpace.Add(element.Id, element);
            }
            
            
            NewScreenElementAdded(null, new ScreenElementEventArgs() {Element = element});
        }

        public void ReplaceExistingElement(ScreenElement element)
        {
            if (ElementsOnWorkSpace.ContainsKey(element.Id))
            {
                var oldElement = ElementsOnWorkSpace[element.Id];
                ElementsOnWorkSpace.Remove(element.Id);
                ElementsOnWorkSpace.Add(element.Id, element);
                ScreenElementReplaced(null, new ReplacingElementEventArgs() { OldElement = oldElement, NewElement = element });
            }
            else
            {
                throw new InvalidOperationException($"Replacing error! Element with ID {element.Id} doesn't exist on WorkSpace!");
            }

        }

        public void DeleteElement(ScreenElement element)
        {
            if (ElementsOnWorkSpace.ContainsKey(element.Id))
            {
                var elementOnWorkspace = ElementsOnWorkSpace[element.Id];
                ElementsOnWorkSpace.Remove(element.Id);
                
                ScreenElementDeleted(null, new ScreenElementEventArgs() { Element = elementOnWorkspace });
            }
            else
            {
                throw new InvalidOperationException($"Deleting error! Element with ID {element.Id} doesn't exist on WorkSpace!");
            }
        }

        (bool XEnabled, bool YEnabled) CheckIfElementsCanBeShifted(List<ScreenElement> elements)
        {
            bool XEnabled = true;
            bool YEnabled = true;

            foreach (var element in CopyPasteBuffer)
            {
                if (element.CoordX + element.Width >= WorkSpaceWidth)
                {
                    XEnabled = false;
                }

                if (element.CoordY + element.Height >= WorkSpaceHeight)
                {
                    YEnabled = false;
                }
            }
            return (XEnabled, YEnabled);
        }

        public List<ScreenElement> CopyElementsInList(List<ScreenElement> elements)
        {
            List<ScreenElement> result = new List<ScreenElement>();
            foreach (var element in elements)
            {
                var type = element.GetType();
                var newItem = (ScreenElement)Activator.CreateInstance(type);
                newItem.InitializeFromAnotherElement(element);
                result.Add(newItem);
            }
            return result;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
