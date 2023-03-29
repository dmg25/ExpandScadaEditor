using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ExpandScadaEditor.ScreenEditor.Items;

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
            }
        }


        public VectorEditorVM()
        {
            // tests!
            Items.Add(new TestItem1VM());
            Items.Add(new TestItem1VM());
            Items.Add(new TestItem2VM());
            Items.Add(new TestItem2VM());





        }












        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
