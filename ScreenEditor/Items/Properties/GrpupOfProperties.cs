using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ExpandScadaEditor.ScreenEditor.Items.Properties
{
    public class GroupOfProperties
    {
        public string GroupTitle { get; set; }

        public bool IsGroupForContent { get; private set; }

        private ObservableCollection<ElementProperty> elementProperties = new ObservableCollection<ElementProperty>();
        public ObservableCollection<ElementProperty> ElementProperties
        {
            get
            {
                return elementProperties;
            }
            set
            {
                elementProperties = value;
            }
        }

        public GroupOfProperties(string title, bool isGroupForContent, ObservableCollection<ElementProperty> properties)
        {
            GroupTitle = title;
            ElementProperties = properties;
            IsGroupForContent = isGroupForContent;
        }
    }
}
