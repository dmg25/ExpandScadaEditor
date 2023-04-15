using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ExpandScadaEditor.Data;

namespace ExpandScadaEditor.ScreenEditor.Items
{
    /*      Plan
     *  - Create dictionary of properties 
     *      - each property must have name,desc,validation
     *      - we must have groups of properties to split them good
     *      - must be notifyed
     *      - fill list here for a while, maybe reading from file after
     *  - Create also properties for signal's binding, not here maybe in each item personally
     *  - Bind these properties manually to the view in each item too
     *  
     * 
     * 
     * */

    /*  Structure
     *  - Properties splitted by groups. Some of them make visual effect immidiately
     *  - Command for selecting with behavior
     *  - Commands for moving
     *  - Commands for resizing with mouth
     *  - Reaction on 2xClick
     *  - Reaction on right click
     *  - reactions on hot keys? Or this must be converted in main editor to command?
     * 
     * 
     * 
     * 
     * 
     * */









    public abstract class BasicVectorItemVM : INotifyPropertyChanged
    {
        /// <summary>
        /// Groups of properties
        /// </summary>
        public Dictionary<string, Dictionary<string, Parameter>> Properties;

        public bool StoredInCatalog { get; set; } = true;

        private Command _selectItem;
        public Command SelectItem
        {
            get;
            protected set;
        }

        private Command _moveItem;
        public Command MoveItem
        {
            get;
            protected set;
        }

        private Command _resizeitem;
        public Command Resizeitem
        {
            get;
            protected set;
        }

        private Command _doubleClick;
        public Command DoubleClick
        {
            get;
            protected set;
        }

        private Command _rightClick;
        public Command RightClick
        {
            get;
            protected set;
        }



        public BasicVectorItemVM()
        {
            Properties = new Dictionary<string, Dictionary<string, Parameter>>();

            Func<int, string> positiveIntValidation =
                delegate (int val)
                {
                    if (val < 0)
                        return "Must be positive!";
                    return String.Empty;
                };

            // Position parameters
            Dictionary<string, Parameter> positionParameters = new Dictionary<string, Parameter>
            {
                {"X", new Parameter<int>("X","Position X", 0, positiveIntValidation)},
                {"Y", new Parameter<int>("Y","Position Y", 0, positiveIntValidation)},
                {"Width", new Parameter<int>("Width","Width", 0, positiveIntValidation)},
                {"Height", new Parameter<int>("Height","Height", 0, positiveIntValidation)}
            };

            Properties.Add("Geometry properties", positionParameters);
        }


        public virtual void CopySettingsFromAnotherVM(BasicVectorItemVM anotherVM)
        {
            // TODO check if we can get not just link to object
            // TODO make it more elegant
            Properties["Geometry properties"]["X"].ObjValue = anotherVM.Properties["Geometry properties"]["X"].ObjValue;
            Properties["Geometry properties"]["Y"].ObjValue = anotherVM.Properties["Geometry properties"]["Y"].ObjValue;
            Properties["Geometry properties"]["Width"].ObjValue = anotherVM.Properties["Geometry properties"]["Width"].ObjValue;
            Properties["Geometry properties"]["Height"].ObjValue = anotherVM.Properties["Geometry properties"]["Height"].ObjValue;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
