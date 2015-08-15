using System;
using System.Collections.Generic;

namespace Gearset.Components {
    public sealed class TreeViewNode {
        public List<TreeViewNode> Nodes;
        public String Name;
        public String FilterName;
        public Object Value;
        public bool Open;

        public TreeViewNode(String name) {
            Name = name;
            FilterName = name.ToLower();
            Nodes = new List<TreeViewNode>();
            Open = false;
        }

        public void Toggle() {
            Open = !Open;
        }
    }
}
