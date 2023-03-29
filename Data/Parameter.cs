using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ExpandScadaEditor.Data
{
    public abstract class Parameter : INotifyPropertyChanged
    {
        /*  - can have any simple datatype - generic
         *  - has name, value, default value, validation rule (some limits maybe too),
         *    description
         * 
         * 
         * 
         * 
         * */


        public string Name 
        {
            get; 
            set; 
        }
        public string Description 
        {
            get;
            set;
        }

        public virtual object ObjValue
        {
            get;
            set;
        }

        public virtual Type DataType
        {
            get;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }


    public class Parameter<T> : Parameter, IDataErrorInfo
    {
        Type _dataType;
        private T _value;
        private T _defaultValue;
        public override Type DataType
        {
            get
            {
                return _dataType;
            }
        }

        public override object ObjValue
        {
            get
            {
                return this._value;
            }
            set
            {


                ObjValue = (T)value; 
                this.OnPropertyChanged();
            }
        }

        public T Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
                this.OnPropertyChanged();
            }
        }

        public readonly Func<T, string> Validation = null;

        public string this[string columnName]
        {
            get
            {
                return Validation(Value);
            }
        }
        public string Error
        {
            get { throw new NotImplementedException(); }
        }

        public Parameter(string name, string description, T defaultValue, Func<T, string> validationFunc = null )
        {
            _dataType = typeof(T);
            Name = name;
            Description = description;
            _defaultValue = defaultValue;
            _value = defaultValue;
            Validation = validationFunc;
        }

        public void RestoreDefaultValue()
        {
            Value = _defaultValue;
        }

    }




}
