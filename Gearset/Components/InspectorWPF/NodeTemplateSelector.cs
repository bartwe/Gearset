using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gearset.Components.InspectorWPF {
    public class NodeTemplateSelector : DataTemplateSelector {
        static readonly Dictionary<Type, CachedTemplate> TypeTemplateMap;
        static readonly CachedTemplate GenericTemplateCache = new CachedTemplate("genericFieldTemplate");
        static readonly CachedTemplate GearConfigTemplateCache = new CachedTemplate("gearConfigTemplate");
        static CachedTemplate _rootTemplateCache = new CachedTemplate("rootTemplate");

        /// <summary>
        /// Static constructor
        /// </summary>
        static NodeTemplateSelector() {
            TypeTemplateMap = new Dictionary<Type, CachedTemplate>();

            // Primitive types
            TypeTemplateMap.Add(typeof(float), new CachedTemplate("floatFieldTemplate"));
            TypeTemplateMap.Add(typeof(double), new CachedTemplate("floatFieldTemplate"));
            TypeTemplateMap.Add(typeof(decimal), new CachedTemplate("floatFieldTemplate"));
            TypeTemplateMap.Add(typeof(char), new CachedTemplate("floatFieldTemplate"));
            TypeTemplateMap.Add(typeof(short), new CachedTemplate("floatFieldTemplate"));
            TypeTemplateMap.Add(typeof(ushort), new CachedTemplate("floatFieldTemplate"));
            TypeTemplateMap.Add(typeof(int), new CachedTemplate("floatFieldTemplate"));
            TypeTemplateMap.Add(typeof(uint), new CachedTemplate("floatFieldTemplate"));
            TypeTemplateMap.Add(typeof(long), new CachedTemplate("floatFieldTemplate"));
            TypeTemplateMap.Add(typeof(ulong), new CachedTemplate("floatFieldTemplate"));
            TypeTemplateMap.Add(typeof(byte), new CachedTemplate("floatFieldTemplate"));
            TypeTemplateMap.Add(typeof(sbyte), new CachedTemplate("floatFieldTemplate"));
            TypeTemplateMap.Add(typeof(bool), new CachedTemplate("boolFieldTemplate"));

            // Other System stuff
            TypeTemplateMap.Add(typeof(String), new CachedTemplate("stringFieldTemplate"));
            TypeTemplateMap.Add(typeof(Enum), new CachedTemplate("enumFieldTemplate"));
            TypeTemplateMap.Add(typeof(void), new CachedTemplate("actionFieldTemplate"));

            // XNA stuff
            TypeTemplateMap.Add(typeof(Vector2), new CachedTemplate("vector2FieldTemplate"));
            TypeTemplateMap.Add(typeof(Vector3), new CachedTemplate("vector3FieldTemplate"));
            TypeTemplateMap.Add(typeof(Quaternion), new CachedTemplate("quaternionFieldTemplate"));
            TypeTemplateMap.Add(typeof(Texture2D), new CachedTemplate("texture2DFieldTemplate"));
            TypeTemplateMap.Add(typeof(Color), new CachedTemplate("colorFieldTemplate"));
            TypeTemplateMap.Add(typeof(Curve), new CachedTemplate("curveFieldTemplate"));

            // Gearset stuff
            TypeTemplateMap.Add(typeof(CollectionMarker), new CachedTemplate("collectionFieldTemplate"));
            TypeTemplateMap.Add(typeof(Texture2DMarker), new CachedTemplate("texture2DMarkerTemplate"));
            TypeTemplateMap.Add(typeof(LineDrawerConfig), new CachedTemplate("clearableGearConfigTemplate"));
            TypeTemplateMap.Add(typeof(LabelerConfig), new CachedTemplate("clearableGearConfigTemplate"));
            TypeTemplateMap.Add(typeof(TreeViewConfig), new CachedTemplate("clearableGearConfigTemplate"));
            TypeTemplateMap.Add(typeof(PlotterConfig), new CachedTemplate("clearableGearConfigTemplate"));
        }

        public override DataTemplate SelectTemplate(Object item, DependencyObject container) {
            var element = container as FrameworkElement;

            if (element != null && item != null) {
                element.DataContext = item;
                var node = item as InspectorNode;
                var nodeType = node.Type;

                // Enums are handled differently
                if (nodeType.IsEnum)
                    nodeType = typeof(Enum);

                //// The root has a especial case.
                //if (node.Parent == null)
                //{
                //    if (rootTemplateCache.DataTemplate == null)
                //        rootTemplateCache.DataTemplate = element.FindResource(rootTemplateCache.Name) as DataTemplate;
                //    return rootTemplateCache.DataTemplate;
                //}


                if (TypeTemplateMap.ContainsKey(nodeType)) {
                    var cache = TypeTemplateMap[nodeType];
                    if (cache.DataTemplate == null)
                        cache.DataTemplate = element.FindResource(cache.Name) as DataTemplate;
                    return cache.DataTemplate;
                }
                if (typeof(GearConfig).IsAssignableFrom(nodeType)) {
                    if (GearConfigTemplateCache.DataTemplate == null)
                        GearConfigTemplateCache.DataTemplate = element.FindResource(GearConfigTemplateCache.Name) as DataTemplate;
                    return GearConfigTemplateCache.DataTemplate;
                }
                if (GenericTemplateCache.DataTemplate == null)
                    GenericTemplateCache.DataTemplate = element.FindResource(GenericTemplateCache.Name) as DataTemplate;
                return GenericTemplateCache.DataTemplate;
            }

            return null;
        }
    }
}
