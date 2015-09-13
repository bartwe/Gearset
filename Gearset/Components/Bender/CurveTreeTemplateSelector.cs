using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Gearset.Components {
    public sealed class CurveTreeTemplateSelector : DataTemplateSelector {
        static readonly Dictionary<Type, CachedTemplate> TypeTemplateMap;

        /// <summary>
        ///     Static constructor
        /// </summary>
        static CurveTreeTemplateSelector() {
            TypeTemplateMap = new Dictionary<Type, CachedTemplate>();

            TypeTemplateMap.Add(typeof(CurveTreeNode), new CachedTemplate("curveTreeNodeTemplate"));
            TypeTemplateMap.Add(typeof(CurveTreeLeaf), new CachedTemplate("curveTreeLeafTemplate"));
        }

        public override DataTemplate SelectTemplate(Object item, DependencyObject container) {
            var element = container as FrameworkElement;

            if (element != null && item != null) {
                var nodeType = item.GetType();

                if (TypeTemplateMap.ContainsKey(nodeType)) {
                    var cache = TypeTemplateMap[nodeType];
                    if (cache.DataTemplate == null)
                        cache.DataTemplate = element.FindResource(cache.Name) as DataTemplate;
                    return cache.DataTemplate;
                }
            }

            return null;
        }
    }
}
