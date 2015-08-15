using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gearset.Components.InspectorWPF {
    public sealed class NodeTemplateSelector : DataTemplateSelector {
        static readonly Dictionary<Type, CachedTemplate> TypeTemplateMap;
        static readonly CachedTemplate GenericTemplateCache = new CachedTemplate("GenericFieldTemplate");
        static readonly CachedTemplate GearConfigTemplateCache = new CachedTemplate("GearConfigTemplate");
        static CachedTemplate _rootTemplateCache = new CachedTemplate("RootTemplate");

        /// <summary>
        /// Static constructor
        /// </summary>
        static NodeTemplateSelector() {
            TypeTemplateMap = new Dictionary<Type, CachedTemplate>();

            // Primitive types
            TypeTemplateMap.Add(typeof(float), new CachedTemplate("FloatFieldTemplate"));
            TypeTemplateMap.Add(typeof(double), new CachedTemplate("FloatFieldTemplate"));
            TypeTemplateMap.Add(typeof(decimal), new CachedTemplate("FloatFieldTemplate"));
            TypeTemplateMap.Add(typeof(char), new CachedTemplate("FloatFieldTemplate"));
            TypeTemplateMap.Add(typeof(short), new CachedTemplate("FloatFieldTemplate"));
            TypeTemplateMap.Add(typeof(ushort), new CachedTemplate("FloatFieldTemplate"));
            TypeTemplateMap.Add(typeof(int), new CachedTemplate("FloatFieldTemplate"));
            TypeTemplateMap.Add(typeof(uint), new CachedTemplate("FloatFieldTemplate"));
            TypeTemplateMap.Add(typeof(long), new CachedTemplate("FloatFieldTemplate"));
            TypeTemplateMap.Add(typeof(ulong), new CachedTemplate("FloatFieldTemplate"));
            TypeTemplateMap.Add(typeof(byte), new CachedTemplate("FloatFieldTemplate"));
            TypeTemplateMap.Add(typeof(sbyte), new CachedTemplate("FloatFieldTemplate"));
            TypeTemplateMap.Add(typeof(bool), new CachedTemplate("BoolFieldTemplate"));

            // Other System stuff
            TypeTemplateMap.Add(typeof(String), new CachedTemplate("StringFieldTemplate"));
            TypeTemplateMap.Add(typeof(Enum), new CachedTemplate("EnumFieldTemplate"));
            TypeTemplateMap.Add(typeof(void), new CachedTemplate("ActionFieldTemplate"));

            // XNA stuff
            TypeTemplateMap.Add(typeof(Vector2), new CachedTemplate("Vector2FieldTemplate"));
            TypeTemplateMap.Add(typeof(Vector3), new CachedTemplate("Vector3FieldTemplate"));
            TypeTemplateMap.Add(typeof(Quaternion), new CachedTemplate("QuaternionFieldTemplate"));
            TypeTemplateMap.Add(typeof(Texture2D), new CachedTemplate("Texture2DFieldTemplate"));
            TypeTemplateMap.Add(typeof(Color), new CachedTemplate("ColorFieldTemplate"));
            TypeTemplateMap.Add(typeof(Curve), new CachedTemplate("CurveFieldTemplate"));

            // Gearset stuff
            TypeTemplateMap.Add(typeof(CollectionMarker), new CachedTemplate("CollectionFieldTemplate"));
            TypeTemplateMap.Add(typeof(Texture2DMarker), new CachedTemplate("Texture2DMarkerTemplate"));
            TypeTemplateMap.Add(typeof(LineDrawerConfig), new CachedTemplate("ClearableGearConfigTemplate"));
            TypeTemplateMap.Add(typeof(LabelerConfig), new CachedTemplate("ClearableGearConfigTemplate"));
            TypeTemplateMap.Add(typeof(TreeViewConfig), new CachedTemplate("ClearableGearConfigTemplate"));
            TypeTemplateMap.Add(typeof(PlotterConfig), new CachedTemplate("ClearableGearConfigTemplate"));
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
