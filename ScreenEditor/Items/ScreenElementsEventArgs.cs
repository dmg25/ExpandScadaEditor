using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpandScadaEditor.ScreenEditor.Items
{
    public class ScreenElementsEventArgs : EventArgs
    {
        public List<ScreenElement> Elements { get; set; }
    }
}
