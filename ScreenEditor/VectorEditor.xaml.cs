using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using ExpandScadaEditor.ScreenEditor.Items;
using System.Windows.Markup;
using System.Collections.ObjectModel;

namespace ExpandScadaEditor.ScreenEditor
{
    /// <summary>
    /// Логика взаимодействия для VectorEditor.xaml
    /// </summary>
    public partial class VectorEditor : UserControl
    {
        protected VectorEditorVM ViewModel
        {
            get { return (VectorEditorVM)Resources["ViewModel"]; }
        }

        Dictionary<string, ScreenElement> elementsOnWorkSpace = new Dictionary<string, ScreenElement>();
        ScreenElement SelectedElement { get; set; }

        public VectorEditor()
        {
            InitializeComponent();


            // TODO For tests, later make it better
            ItemsTemplateSelector itemsTemplateSelector = (ItemsTemplateSelector)Resources["ItemsTemplateSelector"];
            Dictionary<string, DataTemplate> previewTemplates = new Dictionary<string, DataTemplate>
            {
                {nameof(TestItem1VM), CreateTemplateByName(typeof(TestItem1)) },
                {nameof(TestItem2VM), CreateTemplateByName(typeof(TestItem2)) }
            };
            itemsTemplateSelector.previewTemplates = previewTemplates;


            // Create/Load elements VM must be created automatically for each
            // TODO move it to loading process or smth
            elementsOnWorkSpace.Add("first", new TestItem2() { CoordX = 10, CoordY = 20, Name = "first"});



            // Put all elements on the workplace
            foreach (var pair in elementsOnWorkSpace)
            {
                WorkSpace.Children.Add(pair.Value);
                Canvas.SetLeft(pair.Value, pair.Value.CoordX);
                Canvas.SetTop(pair.Value, pair.Value.CoordY);
                pair.Value.PreviewMouseLeftButtonDown += Value_MouseLeftButtonDown;
                pair.Value.PreviewMouseLeftButtonUp += Value_MouseLeftButtonUp;
            }

            

        }

        private void Value_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectedElement = sender as ScreenElement;
        }

        private void Value_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SelectedElement = null;
        }

        DataTemplate CreateTemplateByName(Type viewType)
        {
            string xamlTemplate = $"<DataTemplate> <items:{viewType.Name}/> </DataTemplate>";

            // What can we do simpler in this context?
            var context = new ParserContext();
            context.XamlTypeMapper = new XamlTypeMapper(new string[0]);
            context.XamlTypeMapper.AddMappingProcessingInstruction("items", viewType.Namespace, viewType.Assembly.FullName);

            context.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
            context.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
            context.XmlnsDictionary.Add("items", "items");

            var template = (DataTemplate)XamlReader.Parse(xamlTemplate, context);
            return template;
        }


       






        private void WorkSpace_MouseMove(object sender, MouseEventArgs e)
        {
            // test
            var mousePosition = e.GetPosition(WorkSpace);
            ViewModel.MouseX = mousePosition.X;
            ViewModel.MouseY = mousePosition.Y;

            

            
            if (SelectedElement != null && e.LeftButton == MouseButtonState.Pressed)
            {
                if (elementsOnWorkSpace.ContainsKey(SelectedElement.Name))
                {
                    SelectedElement.CoordX = mousePosition.X;
                    SelectedElement.CoordY = mousePosition.Y;
                    Canvas.SetLeft(SelectedElement, mousePosition.X);
                    Canvas.SetTop(SelectedElement, mousePosition.Y);
                }

            }

        }


    }
}
