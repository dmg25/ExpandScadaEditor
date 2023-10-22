using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ExpandScadaEditor.ScreenEditor.Items.Properties;

namespace ExpandScadaEditor.ScreenEditor.Items
{
    public class ScreenElementContent : UserControl, INotifyPropertyChanged
    {


        /*  - has common properties for each control
         *  - add special group of properties which must be syncronized with ScreenElement
         *      - coordinates
         *      - size
         *      - ID 
         *  - add common properties for all controls, but not for container
         *      - opacity
         *      - ???
         *  - 
         *      
         * 
         * 
         * 
         * 
         * */

        private ObservableCollection<GroupOfProperties> elementPropertyGroups = new ObservableCollection<GroupOfProperties>();
        public ObservableCollection<GroupOfProperties> ElementPropertyGroups
        {
            get
            {
                return elementPropertyGroups;
            }
            set
            {
                elementPropertyGroups = value;
            }
        }


        public ScreenElementContent()
        {
            CreateEditableProperties();
        }




        private void CreateEditableProperties()
        {
            GroupOfProperties newGroup = new GroupOfProperties("ContentCommon", new ObservableCollection<ElementProperty>()
            {
                CreateEditableDependencyProperty<double>(nameof(Opacity), "Opacity", (val) =>
                {
                    if (val < 0 || val > 1)
                    {
                        return "Value must be between 0 and 1";
                    }
                    return string.Empty;
                }),
            });
        }


        //public ElementProperty<T> CreateEditableProperty<T>(
        //    string propertyName,
        //    string description,
        //    Func<T, string> validation = null,
        //    bool canConnectSignal = false,
        //    bool editable = true,
        //    string customName = null
        //    )
        //{
        //    var propertyInfo = this.GetType().GetProperty(propertyName);

        //    ElementProperty<T> newProperty = new ElementProperty<T>(
        //        customName is null ? propertyName : customName,
        //        description,
        //        validation,
        //        canConnectSignal,
        //        editable);

        //    // Events
        //    // This event invokes only if value was changed - to avoid stackoverflow
        //    newProperty.PropertyChangedNotEqual += (sender, e) =>
        //    {
        //        if (e.PropertyName == "Value")
        //        {
        //            this.GetType().GetProperty(propertyName).SetValue(this, ((ElementProperty<T>)sender).Value);
        //        }
        //    };

        //    this.PropertyChanged += (sender, e) =>
        //    {
        //        if (e.PropertyName == propertyName)
        //        {
        //            newProperty.Value = (T)sender.GetType().GetProperty(propertyName).GetValue(sender, null);
        //        }
        //    };

        //    // set on element 

        //    //ElementProperties.Add(newProperty);
        //    return newProperty;

        //}


        public ElementProperty<T> CreateEditableDependencyProperty<T>(
            string dependencyPropertyName,
            string description,
            Func<T, string> validation = null,
            bool canConnectSignal = false,
            bool editable = true,
            string customName = null
            )
        {

            PropertyChangeNotifier notifier = new PropertyChangeNotifier(this, dependencyPropertyName);
            ElementProperty<T> newProperty = new ElementProperty<T>(
                customName is null ? dependencyPropertyName : customName,
                description,
                (T)notifier.Value,
                validation,
                canConnectSignal,
                editable);

            // Events
            // This event invokes only if value was changed - to avoid stackoverflow
            newProperty.PropertyChangedNotEqual += (sender, e) =>
            {
                if (e.PropertyName == "Value")
                {
                    notifier.Value = ((ElementProperty<T>)sender).Value;
                }
            };

            notifier.ValueChanged += (sender, e) =>
            {
                var notifierFromMessage = sender as PropertyChangeNotifier;
                newProperty.Value = (T)notifierFromMessage.Value;
            };

            // set on element 
            return newProperty;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

    }
}
