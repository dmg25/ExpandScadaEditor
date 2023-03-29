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
using ExpandScadaEditor.ScreenEditor.Items;
using System.Windows.Markup;

namespace ExpandScadaEditor.ScreenEditor
{
    /// <summary>
    /// Логика взаимодействия для VectorEditor.xaml
    /// </summary>
    public partial class VectorEditor : UserControl
    {
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
