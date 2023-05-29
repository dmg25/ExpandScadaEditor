using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpandScadaEditor.ScreenEditor.Items
{
    public class ReplacingElementEventArgs : EventArgs
    {
        public ScreenElement OldElement { get; set; }
        public ScreenElement NewElement { get; set; }
    }
}
