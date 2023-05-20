using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpandScadaEditor.ScreenEditor.Items
{
    public class ResizingEventArgs : EventArgs
    {
        public ResizingType ResizingType { get; set; }
        public double NewWidth { get; set; } = double.NaN;
        public double NewHeight { get; set; } = double.NaN;
        public double NewAngle { get; set; } = double.NaN;
    }
}
