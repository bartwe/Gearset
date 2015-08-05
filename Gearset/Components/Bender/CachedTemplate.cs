using System;
using System.Windows;

namespace Gearset.Components {
    class CachedTemplate {
        internal String Name;
        internal DataTemplate DataTemplate;

        internal CachedTemplate(String name) {
            Name = name;
        }
    }
}