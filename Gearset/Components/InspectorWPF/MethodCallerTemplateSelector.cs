using System;
using System.Windows;
using System.Windows.Controls;

namespace Gearset.Components.InspectorWPF {
    public class MethodCallerTemplateSelector : DataTemplateSelector {
        public override DataTemplate SelectTemplate(Object item, DependencyObject container) {
            var element = container as FrameworkElement;

            if (element != null && item != null) {
                element.DataContext = item;

                if (item is MethodCaller)
                    return element.FindResource("methodCallerItemTemplate") as DataTemplate;
                if (item is MethodParamContainer)
                    return element.FindResource("methodParamItemTemplate") as DataTemplate;
            }

            return null;
        }
    }
}
