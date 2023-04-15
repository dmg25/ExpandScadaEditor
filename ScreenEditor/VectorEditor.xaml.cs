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

        Dictionary<string, UserControl> elementsOnWorkSpace = new Dictionary<string, UserControl>();

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


            // There is maybe something in canvas elements, but only as VM, so create views and put on the workspace

            foreach (var elementVm in ViewModel.ElementsOnWorkSpace)
            {
                // TODO ONLY TESTS ONLY TESTS    make selection of view different way (or move somewhere at least)

                if (elementVm is TestItem1VM)
                {
                    var newElement = new TestItem1();
                    newElement.Resources["ViewModel"] = elementVm;
                    newElement.Name = elementVm.UniqueName;
                    WorkSpace.Children.Add(newElement);
                    Canvas.SetLeft(newElement, elementVm.CoordX);
                    Canvas.SetTop(newElement, elementVm.CoordY);
                    elementsOnWorkSpace.Add(elementVm.UniqueName, newElement);
                }
                else if (elementVm is TestItem2VM)
                {
                    var newElement = new TestItem2();
                    newElement.Resources["ViewModel"] = elementVm;
                    newElement.Name = elementVm.UniqueName;
                    WorkSpace.Children.Add(newElement);
                    Canvas.SetLeft(newElement, elementVm.CoordX);
                    Canvas.SetTop(newElement, elementVm.CoordY);
                    elementsOnWorkSpace.Add(elementVm.UniqueName, newElement);
                }

                
            }

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


       







        private void WorkSpace_Drop(object sender, DragEventArgs e)
        {
            // - get mouse coordinates relative to this canvas
            // - create new child item on this canvas with this view model and view. Maybe one model is not enough?
            // - make it selected

            MessageBox.Show("got it");

            //var droppedData = e.Data.GetData(typeof(Card)) as Card;
            ////var target = (sender as ListBoxItem).DataContext as Card;

            //int targetIndex = CardListControl.Items.IndexOf(target);

            //droppedData.Effect = null;
            //droppedData.RenderTransform = null;

            //Items.Remove(droppedData);
            //Items.Insert(targetIndex, droppedData);

            //// remove the visual feedback drag and drop item
            //if (this._dragdropWindow != null)
            //{
            //    this._dragdropWindow.Close();
            //    this._dragdropWindow = null;
            //}





            // If the DataObject contains string data, extract it.
            //if (e.Data.GetDataPresent(DataFormats.StringFormat))
            //{
            //    string dataString = (string)e.Data.GetData(DataFormats.StringFormat);

            //    // If the string can be converted into a Brush,
            //    // convert it and apply it to the ellipse.
            //    BrushConverter converter = new BrushConverter();
            //    if (converter.IsValid(dataString))
            //    {
            //        Brush newFill = (Brush)converter.ConvertFromString(dataString);
            //        circleUI.Fill = newFill;

            //        // Set Effects to notify the drag source what effect
            //        // the drag-and-drop operation had.
            //        // (Copy if CTRL is pressed; otherwise, move.)
            //        if (e.KeyStates.HasFlag(DragDropKeyStates.ControlKey))
            //        {
            //            e.Effects = DragDropEffects.Copy;
            //        }
            //        else
            //        {
            //            e.Effects = DragDropEffects.Move;
            //        }
            //    }
            //}
            //e.Handled = true;
        }

        private void WorkSpace_MouseMove(object sender, MouseEventArgs e)
        {
            // check if left btn pressed and operation mode

            //  check if there is something in buffer
            //  change position every time 

            // test
            var mousePosition = e.GetPosition(WorkSpace);
            ViewModel.MouseX = mousePosition.X;
            ViewModel.MouseY = mousePosition.Y;

            

            
            if (ViewModel.DragDropBuffer != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var vm = ViewModel.DragDropBuffer as BasicVectorItemVM;

                if (elementsOnWorkSpace.ContainsKey(vm.UniqueName))
                {
                    var element = elementsOnWorkSpace[vm.UniqueName];
                    vm.CoordX = mousePosition.X;
                    vm.CoordY = mousePosition.Y;
                    Canvas.SetLeft(element, mousePosition.X);
                    Canvas.SetTop(element, mousePosition.Y);
                }

                
            }










        }























        //DataTemplate CreateTemplate(Type viewModelType, Type viewType)
        //{
        //    const string xamlTemplate = "<DataTemplate DataType=\"{{x:Type vm:{0}}}\"><v:{1} />  </DataTemplate>";
        //    var xaml = String.Format(xamlTemplate, viewModelType.Name, viewType.Name);

        //    var context = new ParserContext();

        //    context.XamlTypeMapper = new XamlTypeMapper(new string[0]);
        //    context.XamlTypeMapper.AddMappingProcessingInstruction("vm", viewModelType.Namespace, viewModelType.Assembly.FullName);
        //    context.XamlTypeMapper.AddMappingProcessingInstruction("v", viewType.Namespace, viewType.Assembly.FullName);

        //    context.XmlnsDictionary.Add("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
        //    context.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
        //    context.XmlnsDictionary.Add("vm", "vm");
        //    context.XmlnsDictionary.Add("v", "v");

        //    var template = (DataTemplate)XamlReader.Parse(xaml, context);
        //    return template;
        //}
    }
}
