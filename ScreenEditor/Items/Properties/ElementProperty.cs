using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
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


    /*  How to connect signal:
     *      - if we able connect signal to parameter - show small button like "..." after value
     *      - if we pressed value - open special screen where user can select signal and stuff
     *      - sent to this screec allowable type of signal (datatype and read/write type)
     *      - window should filter all wrong signals
     *      - but for now just make empty window
     *      - if signal bound - make background of value (or whole line) another light color. 
     *      - user can write any value in field - in will be default value before first reading of signal
     * */

    public abstract class ElementProperty : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual object ValueObj { get; set; }
        public bool CanConnectSignal { get; set; }
        public virtual bool IsSignalAttached { get; set; }
        public bool Editable { get; set; }
        public bool IsDependancyConnected { get; set; }
        public string PropertyNameForXml { get; set; }
        //public DependencyObject ElementDependancyObject { get; set; } = null;
        public virtual event EventHandler ParameterChangedByUser;

        public ConnectedSignalMode SignalMode { get; set; } = ConnectedSignalMode.ReadOnly;

        public virtual Type PropertyType
        {
            get;
        }

        public virtual Signal ConnectedSignal
        {
            get;
            set;
        }

        public virtual Command AttachSignal
        {
            get;
        }

        public virtual Command OnGotFocus
        {
            get;
        }

        public virtual Command OnLostFocus
        {
            get;
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

        static bool PropertiesAreEqual(ElementProperty elementA, ElementProperty elementB)
        {
            if (elementA is null && elementB is null)
            {
                return true;
            }
            else if (elementA is not null && elementB is null || elementA is null && elementB is not null)
            {
                return false;
            }

            return elementA.Name == elementB.Name && elementA.Description == elementB.Description
                && elementA.Description == elementB.Description && elementA.IsSignalAttached == elementB.IsSignalAttached
                && elementA.ConnectedSignal == elementB.ConnectedSignal && elementA.ConnectedSignal == elementB.ConnectedSignal
                && elementA.ValueObj == elementB.ValueObj;
        }

        public static bool operator ==(ElementProperty elementA, ElementProperty elementB)
        {

            return PropertiesAreEqual(elementA, elementB);
        }

        public static bool operator !=(ElementProperty elementA, ElementProperty elementB)
        {
            return !PropertiesAreEqual(elementA, elementB);
        }

    }


    public class ElementProperty<T> : ElementProperty, IDataErrorInfo
    {
        // There is a problem:
        // On loading we reset Value a lot of times
        // On selecting we call validation from xaml many times
        // how to get situation when user put a value and seccessfully???

        // 

        private bool usersInputStarted;
        private T preValue;
        private T tmpValueForValidationMessage;

        public override event EventHandler ParameterChangedByUser;

        private T _value;
        public T Value
        {
            get
            {
                return this._value;
            }
            set
            {
                tmpValueForValidationMessage = value;
                // Additional check - validation not only in view, but here too. Otherwise value will be written anyway
                if (validation is not null && validation((T)value) != string.Empty)
                {
                    return;
                }

                if (!EqualityComparer<T>.Default.Equals(_value, (T)value))
                {
                    this._value = (T)value;
                    OnPropertyChangedNotEqual();
                }
                else
                {
                    this._value = (T)value;
                }

                this.OnPropertyChanged();
            }
        }

        public override Type PropertyType
        {
            get
            {
                return typeof(T);
            }
        }

        public override object ValueObj
        {
            get => Value;
            set
            {
                Value = (T)value;
            }
        }

        private bool isSignalAttached;
        public override bool IsSignalAttached
        {
            get
            {
                return isSignalAttached;
            }
            set
            {
                isSignalAttached = value;
                OnPropertyChanged();
            }
        }

        private Signal connectedSignal;
        public override Signal ConnectedSignal
        {
            get
            {
                return connectedSignal;
            }
            set
            {
                connectedSignal = value;
                IsSignalAttached = connectedSignal is not null;
                OnPropertyChanged();
            }
        }

        private readonly Func<T, string> validation = null;

        public ElementProperty(string name, 
            string description, 
            Func<T, string> validation = null,
            bool canConnectSignal = false, 
            bool editable = true,
            string propertyNameForXml = null)
        {
            Name = name;
            Description = description;
            this.validation = validation;
            CanConnectSignal = canConnectSignal;
            Editable = editable;
            PropertyNameForXml = propertyNameForXml;
        }

        public ElementProperty(string name,
            string description,
            T initialValue, 
            Func<T, string> validation = null,
            bool canConnectSignal = false,
            bool editable = true,
            string propertyNameForXml = null)
            : this (name, description, validation, canConnectSignal, editable, propertyNameForXml)
        {
            _value = initialValue;
            OnPropertyChanged(nameof(Value));
        }


        // For reflection
        public ElementProperty()
        {

        }


        private Command attachSignal;
        public override Command AttachSignal
        {
            get
            {
                return attachSignal ??
                    (attachSignal = new Command(obj =>
                    {
                        Signal preSignal = ConnectedSignal;
                        var attachSignalWindow = new PropertySignalSelection(this);
                        attachSignalWindow.ShowDialog();

                        if (preSignal != ConnectedSignal)
                        {
                            ParameterChangedByUser?.Invoke(this, new EventArgs());
                        }

                    },
                    obj =>
                    {
                        return CanConnectSignal;
                    }));
            }
        }

        private Command onGotFocus;
        public override Command OnGotFocus
        {
            get
            {
                return onGotFocus ??
                    (onGotFocus = new Command(obj =>
                    {
                        // if flag Entering not set
                        // set flag Entering started 
                        // set tmp T value before changes

                        if (!usersInputStarted)
                        {
                            preValue = Value;
                            usersInputStarted = true;
                        }


                    },
                    obj =>
                    {
                        return Editable;
                    }));
            }
        }

        
        private Command onLostFocus;
        public override Command OnLostFocus
        {
            get
            {
                return onLostFocus ??
                    (onLostFocus = new Command(obj =>
                    {
                        // if flag entering was
                        // and new value is not like tmp
                        // shot event user action
                        // drop entering flag

                        if (usersInputStarted && !EqualityComparer<T>.Default.Equals(preValue, Value))
                        {
                            ParameterChangedByUser?.Invoke(this, new EventArgs());
                            usersInputStarted = false;
                        }
                    },
                    obj =>
                    {
                        return Editable;
                    }));
            }
        }

        public string this[string columnName]
        {
            get
            {
                return validation is null ? String.Empty : validation(tmpValueForValidationMessage);
            }
        }

        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        






    }
}
