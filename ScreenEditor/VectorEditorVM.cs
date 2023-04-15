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
    /*      PLAN
     *  - Create user basic user control which represents any shape/icon, contains common properties , xaml code for rendering
     *  - Create a couple of simple items based on this user control
     *      - first just straight, but after - try to load dynamically from some dll or even external xml file
     *  - Create collection of these elements here and init it on start. 
     *  - show this collection on the side-panel 
     *  - create logic with some binding to take this item and put on the canvas panel
     * 
     *  - create properties panel for selected item (even canvas) and showing on another side panel
     *  - create validation on each parameter and reaction on changing too
     * 
     * 
     * 
     * */


    public class VectorEditorVM : INotifyPropertyChanged
    {
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

        public ObservableCollection<BasicVectorItemVM> ElementsOnWorkSpace { get; set; } = new ObservableCollection<BasicVectorItemVM>();


        public object DragDropBuffer { get; set; }

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
        // -------------------------------------------------------

        public VectorEditorVM()
        {
            // tests!
            Items.Add(new TestItem1VM());
            Items.Add(new TestItem1VM());
            Items.Add(new TestItem2VM());
            Items.Add(new TestItem2VM());


            // TODO move it to loading process or smth
            ElementsOnWorkSpace.Add(new TestItem1VM() { CoordX = 10, CoordY = 20, UniqueName = "first"});

            foreach (var item in ElementsOnWorkSpace)
            {
                item.PropertyChanged += Element_PropertyChanged;

            }


        }


        private void Element_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = sender as BasicVectorItemVM;
            if (e.PropertyName == nameof(vm.IsDragged))
            {
                if (vm.IsDragged)
                {
                    DragDropBuffer = sender;
                }
                else
                {
                    DragDropBuffer = null;
                }

            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
