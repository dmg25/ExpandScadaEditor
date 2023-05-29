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
        public event EventHandler<ReplacingElementEventArgs> ScreenElementReplaced;
        

        private List<BasicVectorItemVM> items = new List<BasicVectorItemVM>();
        public List<BasicVectorItemVM> Items 
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

                        foreach (var element in undoElements)
                        {
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
                        return UndoRedo.UndoIsPossible();
                    }));
            }
        }















        public VectorEditorVM()
        {
            
        }

        public void Initialize()
        {
            // tests!
            Items.Add(new TestItem1VM());
            Items.Add(new TestItem1VM());
            Items.Add(new TestItem2VM());
            Items.Add(new TestItem2VM());


            // Create/Load elements VM must be created automatically for each
            // TODO move it to loading process or smth
            AddNewScreenElement(new TestItem2() { CoordX = 10, CoordY = 20, Name = "first", Width = 50, Height = 50 });
            AddNewScreenElement(new TestItem2() { CoordX = 100, CoordY = 100, Name = "second", Width = 50, Height = 50 });
            AddNewScreenElement(new TestItem2() { CoordX = 100, CoordY = 200, Name = "third", Width = 50, Height = 50 });
        }

        public void AddNewScreenElement(ScreenElement element)
        {
            int newId = idForNewScreenElement++;
            element.Id = newId;
            ElementsOnWorkSpace.Add(newId, element);
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

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
