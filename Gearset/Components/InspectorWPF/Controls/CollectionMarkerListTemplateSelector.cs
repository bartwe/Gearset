using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xna.Framework.Graphics;

namespace Gearset.Components.InspectorWPF {
    public sealed class CollectionMarkerListTemplateSelector : DataTemplateSelector {
        static readonly CachedTemplate Texture2DTemplateCache = new CachedTemplate("textureTemplate");
        static readonly CachedTemplate GenericTemplateCache = new CachedTemplate("genericTemplate");

        /// <summary>
        /// Static constructor
        /// </summary>
        static CollectionMarkerListTemplateSelector() {}

        public override DataTemplate SelectTemplate(Object item, DependencyObject container) {
            var element = container as FrameworkElement;

            if (element != null && item != null) {
                element.DataContext = item;

                if (item is Texture2D) {
                    if (Texture2DTemplateCache.DataTemplate == null)
                        Texture2DTemplateCache.DataTemplate = element.FindResource(Texture2DTemplateCache.Name) as DataTemplate;
                    return Texture2DTemplateCache.DataTemplate;
                }
                if (GenericTemplateCache.DataTemplate == null)
                    GenericTemplateCache.DataTemplate = element.FindResource(GenericTemplateCache.Name) as DataTemplate;
                return GenericTemplateCache.DataTemplate;
            }

            return null;
        }
    }
}
