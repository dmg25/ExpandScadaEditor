using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace ExpandScadaEditor.ScreenEditor
{
    public class ItemsTemplateSelector : DataTemplateSelector
    {
        public DataTemplate errorTemplate { get; set; }
        public Dictionary<string, DataTemplate> previewTemplates = new Dictionary<string, DataTemplate>();

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            // это место нужно переделать. Можно сделать шаблон специальный для случая, если не удалось что-то подгрузить, написать там мессадж
            DataTemplate selectedTemplate;

            string typeVmPath = item?.GetType().Name.ToString();

            // сюда приходит полный путь к VM от девайса
            // надо как-то получить список всех путей к VM и в цикле проверять совпадение

            try
            {
                selectedTemplate = previewTemplates[typeVmPath];
            }
            catch
            {
                selectedTemplate = errorTemplate;
            }

            return selectedTemplate;
        }




    }
}
