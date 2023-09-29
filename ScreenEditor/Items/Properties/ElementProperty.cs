using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Common.Gateway;

namespace ExpandScadaEditor.ScreenEditor.Items.Properties
{
    /*
     *  - Create generic object for properties
     *      - must support any simple datatype
     *          - think about colour, how to contain this?
     *      - provide basic validation based on datatype - automatically
     *      - allow for user add his validation rules via method
     *          - use this validation to show to user that he can not set something as message and colour maybe
     *          - on runtime just do not change property and write to debug this message. NO CRUSH  
     *      - each property can be connectable or not to some signal - user have to set it
     *          - if parameter means readonly to Signal - add without special logic - only with datatype checking
     *          - if signal has to be written by button or something like this - activate checking logic if signal attached already - forbidden
     *          - on attaching open some window for selecting signals from table/list , but this later
     *      - description of property
     *  - Create view for each property object
     *      - try create only one view first and select template depended on datatype 
     *      - if signal is connected - show it as border/background color, but user can set default or his values as default
     *        to show something before signal send any value
     *      - so it sould be some button for signal attaching
     *      - show name of attached signal as popup tip if there is one
     *      - in left side show the name - popup tip if user can not see full name
     *      - in the right side - value
     *      - in validation is not ok - show popup error and bad backgroung
     *          - after user pressed esc or just left the field - drop thic color and revert pre-value
     * */

    public abstract class ElementProperty : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public bool CanConnectSignal { get; set; }
        public bool Editable { get; set; }
        public ConnectedSignalMode SignalMode { get; set; } = ConnectedSignalMode.ReadOnly;

        public virtual object Value
        {
            get;
            set;
        }

        public virtual Type PropertyType
        {
            get;
        }

        public virtual Signal ConnectedSignal
        {
            get;
            set;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChangedNotEqual;
        /// <summary>
        /// Will be invoked only if the new value is not equal.
        /// </summary>
        /// <param name="prop"></param>
        public void OnPropertyChangedNotEqual([CallerMemberName] string prop = "")
        {
            PropertyChangedNotEqual?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }


    public class ElementProperty<T> : ElementProperty, IDataErrorInfo
    {
        /*      Binding
         *  - we have to add subscribe on 
         * 
         * 
         * */



        private T _value;
        public override object Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = (T)value;

                if (!EqualityComparer<T>.Default.Equals(_value, (T)value))
                {
                    OnPropertyChangedNotEqual();
                }

                
                this.OnPropertyChanged();
            }
        }

        

        private Func<T, string> validation = null;



        public ElementProperty(string name, 
            string description, 
            Func<T, string> validation = null,
            bool canConnectSignal = false, 
            bool editable = true)
        {
            Name = name;
            Description = description;
            this.validation = validation;
            CanConnectSignal = canConnectSignal;
            Editable = editable;
        }

        // For reflection
        public ElementProperty()
        {

        }



        public string this[string columnName]
        {
            get
            {
                return validation is null ? String.Empty : validation(_value);
            }
        }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }





    }












}
