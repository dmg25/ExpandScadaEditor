﻿using System;
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
using ExpandScadaEditor.ScreenEditor.WorkspaceHelperControls;

namespace ExpandScadaEditor.ScreenEditor.Items
{
    /// <summary>
    /// Логика взаимодействия для ScreenElementContainer.xaml
    /// </summary>
    public partial class ScreenElementContainer : ScreenElement
    {
        public ScreenElementContainer(ScreenElementContent contentElement) 
            : base(contentElement)
        {
            InitializeComponent();

            var newItem = (ScreenElementContent)Activator.CreateInstance(contentElement.GetType());

            foreach (var group in newItem.ElementPropertyGroups)
            {
                // subscribe on changing events by the way, because it was made outside of container
                foreach (var parameter in group.ElementProperties)
                {
                    parameter.ParameterChangedByUser += base.NewProperty_ParameterChangedByUser;
                }

                base.ElementPropertyGroups.Add(group);
            }

            RootContainer.Children.Insert(0, newItem);

            //after UNDO/ REDO action clean properties window(check notification?)
            // UNDO / REDO if it was replacing - do not drop selection - try to hold it(just update)
            //   then finally move to saving method...
        }
    }
}
