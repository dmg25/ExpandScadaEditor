using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ExpandScadaEditor.ScreenEditor.Items;
using System.Collections.ObjectModel;

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
        public event EventHandler<ReplacingElementEventArgs> ScreenElementReplaced;


        //private List<BasicVectorItemVM> items = new List<BasicVectorItemVM>();
        //public List<BasicVectorItemVM> Items 
        //{ 
        //    get
        //    {
        //        return items;
        //    }
        //    set
        //    {
        //        items = value;
        //        NotifyPropertyChanged();
        //    }
        //}

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
                        var undoElements = UndoRedo.Undo();

                        foreach (var element in undoElements.elements)
                        {
                            if (ElementsOnWorkSpace.ContainsKey(element.Id))
                            {
                                if (undoElements.werePasted)
                                {
                                    DeleteElement(element);
                                }
                                else
                                {
                                    ReplaceExistingElement(element);
                                }
                            }
                            else
                            {
                                AddNewScreenElement(element);
                            }
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

                        foreach (var element in redoElements.elements)
                        {
                            if (redoElements.werePasted)
                            {
                                AddNewScreenElement(element,true);
                                return;
                            }

                            if (ElementsOnWorkSpace.ContainsKey(element.Id))
                            {

                                ReplaceExistingElement(element);
                            }
                            else
                            {
                                AddNewScreenElement(element);
                            }
                        }
                    },
                    obj =>
                    {
                        return UndoRedo.RedoIsPossible();
                    }));
            }
        }











        public VectorEditorVM()
        {
            
        }

        public void Initialize()
        {
            // tests!
            //Items.Add(new TestItem2VM());
            //Items.Add(new TestItem2VM());

            Items.Add(new TestItem2());
            Items.Add(new TestItem2());


            // Create/Load elements VM must be created automatically for each
            // TODO move it to loading process or smth
            AddNewScreenElement(new TestItem2() { CoordX = 10, CoordY = 20, Name = "first"});
            AddNewScreenElement(new TestItem2() { CoordX = 100, CoordY = 100, Name = "second" });
            AddNewScreenElement(new TestItem2() { CoordX = 100, CoordY = 200, Name = "third"});
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

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
