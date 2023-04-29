using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace ExpandScadaEditor.ScreenEditor.Items
{
    public class ScreenElement : UserControl
    {
        private bool isDragged;
        public bool IsDragged
        {
            get
            {
                return isDragged;
            }
            set
            {
                isDragged = value;
            }
        }

        private double coordX;
        public double CoordX
        {
            get
            {
                return coordX;
            }
            set
            {
                coordX = value;
                //NotifyPropertyChanged();
            }
        }

        private double coordY;
        public double CoordY
        {
            get
            {
                return coordY;
            }
            set
            {
                coordY = value;
                //NotifyPropertyChanged();
            }
        }

        private Border borderAround;
        public Border BorderAround
        {
            get
            {
                return borderAround;
            }
            set
            {
                borderAround = value;
                //NotifyPropertyChanged();
            }
        }


        public ScreenElement()
        {

        }
    }
}
