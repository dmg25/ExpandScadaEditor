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
using System.Windows.Controls;
using ExpandScadaEditor.ScreenEditor.Items.Catalog;
using System.Windows.Markup;
using System.Windows;
using System.Reflection;
using System.IO;
using Microsoft.Win32;

namespace ExpandScadaEditor.ScreenEditor
{
    public class VectorEditorVM : INotifyPropertyChanged
    {



        /*      Loading
         * 
         * 
         * 
         * */
        public const string ROOT_CONTENT_KEY_NAME = "ElementToSave";

        public UndoRedoContainer UndoRedo = new UndoRedoContainer();

        public event EventHandler<ScreenElementEventArgs> NewScreenElementAdded;
        public event EventHandler<ScreenElementEventArgs> ScreenElementDeleted;
        public event EventHandler SelectedElementsDeleted;
        public event EventHandler<ReplacingElementEventArgs> ScreenElementReplaced;
        public event EventHandler<ScreenElementsEventArgs> SelectTheseElements;
        public event EventHandler ZoomChanged;

         
        private ObservableCollection<ScreenElement> selectedElements = new ObservableCollection<ScreenElement>();
        public ObservableCollection<ScreenElement> SelectedElements
        {
            get
            {
                return selectedElements;
            }
            set
            {
                selectedElements = value;
                NotifyPropertyChanged();
            }
        }

        private WorkspaceCanvas workspace;
        public WorkspaceCanvas Workspace
        {
            get
            {
                return workspace;
            }
            set
            {
                workspace = value;
                NotifyPropertyChanged();
            }
        }

        

        private bool isWorkspaceSelected;
        public bool IsWorkspaceSelected
        {
            get
            {
                return isWorkspaceSelected;
            }
            set
            {
                isWorkspaceSelected = value;
                NotifyPropertyChanged();
            }
        }

        //internal Dictionary<int, ScreenElement> SelectedElements = new Dictionary<int, ScreenElement>();
        internal ObservableCollection<ScreenElement> CopyPasteBuffer = new ObservableCollection<ScreenElement>();

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

        private double zoomCoef = 1;
        public double ZoomCoef
        {
            get
            {
                return zoomCoef;
            }
            set
            {
                zoomCoef = value;
                //ChangeZoomForAllElements(zoomCoef);
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
                        //SelectedElements.ForEach(x => ElementsOnWorkSpace.Remove(x.Id));
                        foreach (var item in SelectedElements)
                        {
                            ElementsOnWorkSpace.Remove(item.Id);
                        }
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
                        //CopyElementsInList(SelectedElements).ForEach(x => CopyPasteBuffer.Add(x));
                        foreach (var item in CopyElementsInList(SelectedElements))
                        {
                            CopyPasteBuffer.Add(item);
                        }
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
                            //CopyPasteBuffer.ForEach(x => x.CoordX += 5);
                            foreach (var item in CopyPasteBuffer)
                            {
                                item.CoordX += 5;
                            }
                        }
                        if (ShiftingPossibility.YEnabled)
                        {
                            //CopyPasteBuffer.ForEach(x => x.CoordY += 5);
                            foreach (var item in CopyPasteBuffer)
                            {
                                item.CoordY += 5;
                            }
                        }

                        List<ScreenElement> elementsForSelection = new List<ScreenElement>();
                        //CopyElementsInList(CopyPasteBuffer).ForEach(x => { AddNewScreenElement(x); elementsForSelection.Add(x); });
                        foreach (var item in CopyPasteBuffer)
                        {
                            AddNewScreenElement(item); elementsForSelection.Add(item);
                        }
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

        private Command zoomIn;
        public Command ZoomIn
        {
            get
            {
                return zoomIn ??
                    (zoomIn = new Command(obj =>
                    {
                        ZoomCoef += 0.1;
                        ZoomChanged(null, new EventArgs());
                    },
                    obj =>
                    {
                        return true;
                    }));
            }
        }

        private Command zoomOut;
        public Command ZoomOut
        {
            get
            {
                return zoomOut ??
                    (zoomOut = new Command(obj =>
                    {
                        ZoomCoef -= 0.1;
                        ZoomChanged(null, new EventArgs());
                    },
                    obj =>
                    {
                        return ZoomCoef > 0.1;
                    }));
            }
        }

        private Command resetZoom;
        public Command ResetZoom
        {
            get
            {
                return resetZoom ??
                    (resetZoom = new Command(obj =>
                    {
                        ZoomCoef = 1;
                        ZoomChanged(null, new EventArgs());
                    },
                    obj =>
                    {
                        return true;
                    }));
            }
        }

        private Command saveScreen;
        public Command SaveScreen
        {
            get
            {
                return saveScreen ??
                    (saveScreen = new Command(obj =>
                    {
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.Filter = "Xaml file (.*xaml)|*.xaml";
                        if (saveFileDialog.ShowDialog() == true)
                        {
                            SaveScreenInXamlFile(saveFileDialog.FileName);
                        }
                            

                        
                    },
                    obj =>
                    {
                        return true;
                    }));
            }
        }

        public VectorEditorVM()
        {
            
        }

        public void Initialize(WorkspaceCanvas workspace)
        {

            Workspace = workspace;
            //Items.Add(new TestItem2() { CatalogMode = true, Id = -111});
            //Items.Add(new TestItem2() { CatalogMode = true, Id = -222 });

            Items.Add(new ScreenElementContainer(new TestItem2()) { CatalogMode = true, Id = -111, Width = 50, Height = 50 });
            Items.Add(new ScreenElementContainer(new TestItem2()) { CatalogMode = true, Id = -222, Width = 50, Height = 50 });

            Items.Add(new ScreenElementContainer(new RectangleElement()) { CatalogMode = true, Id = -333, Width = 50, Height = 50 });
            Items.Add(new ScreenElementContainer(new RectangleElement()) { CatalogMode = true, Id = -444, Width = 50, Height = 50 });


            //Items.Add(new RectangleElement() { CatalogMode = true, Id = -333, Width = 50, Height = 50 });
            //Items.Add(new RectangleElement() { CatalogMode = true, Id = -444, Width = 50, Height = 50 });

            // Create/Load elements VM must be created automatically for each
            // TODO move it to loading process or smth
            //AddNewScreenElement(new TestItem2() { CoordX = 10, CoordY = 20, Name = "first"});
            //AddNewScreenElement(new TestItem2() { CoordX = 100, CoordY = 100, Name = "second" });
            //AddNewScreenElement(new TestItem2() { CoordX = 100, CoordY = 200, Name = "third"});

            AddNewScreenElement(new ScreenElementContainer(new TestItem2()) { CoordX = 10, CoordY = 20, Name = "first", Width = 50, Height = 50 });
            AddNewScreenElement(new ScreenElementContainer(new TestItem2()) { CoordX = 100, CoordY = 100, Name = "second", Width = 50, Height = 50 });
            AddNewScreenElement(new ScreenElementContainer(new TestItem2()) { CoordX = 100, CoordY = 200, Name = "third", Width = 50, Height = 50 });



            AddNewScreenElement(new ScreenElementContainer(new RectangleElement()) { CoordX = 300, CoordY = 300, Name = "rect1", Width = 50, Height = 50 });

            //AddNewScreenElement(new RectangleElement() { CoordX = 300, CoordY = 300, Name = "rect1", Width = 50, Height = 50 }) ;

            //AddNewScreenElement(new ScreenElementContainer( new RectangleElement() { CoordX = 300, CoordY = 300, Name = "rect1", Width = 50, Height = 50 }));





            //MyCommand.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
        }

        public void AddNewScreenElement(ScreenElement element, bool useOldId = false)
        {
            element.ZoomCoef = ZoomCoef;
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

            element.ParametersChangedByUser += Element_ParametersChangedByUser;
            NewScreenElementAdded(null, new ScreenElementEventArgs() {Element = element});
        }

        private void Element_ParametersChangedByUser(object sender, EventArgs e)
        {
            UndoRedo.NewUserAction(ElementsOnWorkSpace);
        }

        public void ReplaceExistingElement(ScreenElement element)
        {
            if (ElementsOnWorkSpace.ContainsKey(element.Id))
            {
                element.ZoomCoef = ZoomCoef;
                var oldElement = ElementsOnWorkSpace[element.Id];
                oldElement.ParametersChangedByUser -= Element_ParametersChangedByUser;
                element.ParametersChangedByUser += Element_ParametersChangedByUser;
                ElementsOnWorkSpace.Remove(element.Id);
                ElementsOnWorkSpace.Add(element.Id, element);

                // update selected element
                var selectedElementIndex = SelectedElements.IndexOf(oldElement);
                if (selectedElementIndex >= 0)
                {
                    SelectedElements[selectedElementIndex] = element;
                    element.ShowResizeBorder();
                }

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

        (bool XEnabled, bool YEnabled) CheckIfElementsCanBeShifted(ObservableCollection<ScreenElement> elements)
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

        public ObservableCollection<ScreenElement> CopyElementsInList(ObservableCollection<ScreenElement> elements)
        {
            ObservableCollection<ScreenElement> result = new ObservableCollection<ScreenElement>();
            foreach (var element in elements)
            {
                var type = element.GetType();
                var newItem = (ScreenElement)Activator.CreateInstance(type, element.ElementContent);
                newItem.InitializeFromAnotherElement(element);
                result.Add(newItem);
            }
            return result;
        }

        //void ChangeZoomForAllElements(double zoomCoef)
        //{
        //    foreach (var pair in ElementsOnWorkSpace)
        //    {
        //        pair.Value.ZoomCoef = zoomCoef;
        //    }
        //}


        private void SaveScreenInXamlFile(string filePath)
        {
            /*      Saving
         *      
         *      - start build clone of current screen - create again without trash to read clear xaml for saving later
         *          - create main canvas first - take all main properties manually - give a name to canvas
         *              - later to canvas properties can be connected signals
         *          - for each element on workspace 
         *              - take content of container and recreate it again without container 
         *              - try to add properties of content automatically - via copying or smth. If not - manually from list with groups
         *              - take care about Z index !!! don't implemented yet - just copy canvas attached property???
         *              - check name, if name is empty - generate with ID
         *              - create node for attached signals - add to some list
         *              - add created elemnt on canvas
         *          - save canvas as xaml into string
         *          - add special section with signals
         *          - save to file
         * */

            // As a test first
            Canvas workspace = new Canvas();

            // set personal settings of workspace
            foreach (var propertyGroup in Workspace.ElementPropertyGroups)
            {
                foreach (var property in propertyGroup.ElementProperties)
                {
                    PropertyInfo propertyToSave = workspace.GetType().GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance);
                    propertyToSave.SetValue(workspace, property.ValueObj);
                }
            }

            foreach (var pair in ElementsOnWorkSpace)
            {
                // TODO is it necessary to make a copy?
                var type = pair.Value.GetType();
                var container = (ScreenElement)Activator.CreateInstance(type, pair.Value.ElementContent);
                container.InitializeFromAnotherElement(pair.Value);

                var content = container.ElementContent;

                // Set settings of container on content
                // because of zooming we have to set position/size parameters manually

                // get root of content 
                var rootToSave = (FrameworkElement)content.FindName(ROOT_CONTENT_KEY_NAME);

                content.Content = null;

                var xamlCode1 = XamlWriter.Save(rootToSave);

                Canvas.SetLeft(rootToSave, container.CoordX);
                Canvas.SetTop(rootToSave, container.CoordY);

                rootToSave.Width = container.Width;
                rootToSave.Height = container.Height;

                if (string.IsNullOrEmpty(container.Name))
                {
                    rootToSave.Name = $"Element_ID_{container.Id}";
                }
                else
                {
                    rootToSave.Name = container.Name;
                }

                // set personal settings of content

                foreach (var propertyGroup in container.ElementPropertyGroups)
                {
                    if (propertyGroup.IsGroupForContent)
                    {
                        foreach (var property in propertyGroup.ElementProperties)
                        {
                            PropertyInfo propertyToSave = rootToSave.GetType().GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance);
                            propertyToSave.SetValue(rootToSave, property.ValueObj);

                            // TODO make it like this and write errors to log/user, but now we want to see any error
                            //if (null != opacityProperty)
                            //{
                            //    opacityProperty.SetValue(rootToSave, 0.5);
                            //}
                        }
                    }
                }

                workspace.Children.Add(rootToSave);
            }

            var xamlCode = XamlWriter.Save(workspace);

            List<string> outputLines = new List<string>();
            outputLines.Add(xamlCode);
            outputLines.Add("<EditorBlockForConnections>");
            outputLines.Add("</EditorBlockForConnections>");

            using (StreamWriter outputFile = new StreamWriter(filePath))
            {

                foreach (string line in outputLines)
                    outputFile.WriteLine(line);
            }

        }


        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}
