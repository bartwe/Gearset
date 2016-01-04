using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Gearset.Components.InspectorWPF {
    public sealed class InspectorNode : INotifyPropertyChanged {
        /// <summary>
        ///     This list contains a list of Types that
        ///     contain extension methods that we will call
        ///     on types.
        /// </summary>
        public static List<Type> ExtensionMethodTypes = new List<Type>();

        readonly bool _hideCantWriteIcon;
        TreeViewItem _uiContainer;
        // Since containers change when filtering, we save here if the previous container was expaneded
        // so we can reexpand to restore the previous state.
        bool _wasPreviousContainerExpanded;
        Object _rootTarget;
        bool _userModified;

        /// <summary>
        ///     The VisualItem that can Update the variable
        ///     and Update the UI
        /// </summary>
        internal VisualItemBase VisualItem;

        bool _updating;
        bool _force;
        bool _isShowingPrivate;
        String _friendlyName;
        Type _type;
        CollectionViewSource _childrenView;
        bool _isPrivate;
        // Helper methods
        Setter _setter;
        Getter _getter;
        Random _random = new Random();

        /// <summary>
        ///     Only set for Nodes of Type Void (methods) (no getter nor setter).
        /// </summary>
        public MethodInfo Method;

        /// <summary>
        ///     Use this constructor to create child nodes.
        /// </summary>
        /// <param name="type">The type of the field this node represets</param>
        /// <param name="name">The name of the field this node represets</param>
        /// <param name="setter">Helper delegate to set the value of the field.</param>
        /// <param name="getter">Helper delegate to get the value of the field.</param>
        public InspectorNode(InspectorNode parent, Type type, String name, Setter setter, Getter getter, bool hideCanWriteIcon) {
            Children = new ObservableCollection<InspectorNode>();
            Methods = new ObservableCollection<InspectorNode>();
            Name = name;
            Type = type;
            _setter = setter;
            _getter = getter;
            Parent = parent;

            Updating = true;
            _hideCantWriteIcon = hideCanWriteIcon;
        }

        /// <summary>
        ///     Use this constructor to create the root node
        /// </summary>
        /// <param name="type">The type of the field this node represets</param>
        /// <param name="name">The name of the field this node represets</param>
        /// <param name="setter">Helper delegate to set the value of the field.</param>
        /// <param name="getter">Helper delegate to get the value of the field.</param>
        public InspectorNode(Object target, String name, bool autoExpand) {
            Children = new ObservableCollection<InspectorNode>();
            Methods = new ObservableCollection<InspectorNode>();
            RootTarget = target;
            Name = name;
            Type = target.GetType();

            // The root object is direcly accesable
            IsProperty = false;

            AutoExpand = autoExpand;
        }

        /// <summary>
        ///     The TreeViewNode which holds this node.
        /// </summary>
        public TreeViewItem UiContainer {
            get { return _uiContainer; }
            set {
                if (_uiContainer != null) {
                    _uiContainer.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
                    _wasPreviousContainerExpanded = _uiContainer.IsExpanded;
                }
                _uiContainer = value;
                if (_uiContainer != null) {
                    _uiContainer.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
                    _uiContainer.IsExpanded = _wasPreviousContainerExpanded;
                    ItemContainerGenerator_StatusChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        ///     Returns the root of the tree.
        /// </summary>
        public InspectorNode Root {
            get {
                var n = this;
                while (n.Parent != null)
                    n = n.Parent;
                return n;
            }
        }

        /// <summary>
        ///     This value will only be set in the root node.
        /// </summary>
        internal Object RootTarget {
            get { return _rootTarget; }
            set {
                _rootTarget = value;
                Type = value.GetType();
            }
        }

        /// <summary>
        ///     Returns true if the node is an ExtraNode, that means that it does not correspond
        ///     to any field or property of the parent but is added as an extra control to better
        ///     manipulate the parent.
        /// </summary>
        public bool IsExtraNode { get; private set; }

        /// <summary>
        ///     Will turn true when the user modify this property through the UI.
        /// </summary>
        public bool UserModified {
            get { return _userModified; }
            private set {
                _userModified = value;
                OnPropertyChanged("UserModified");
            }
        }

        /// <summary>
        ///     If true, the UI will be updated every frame to reflect
        ///     the node value.
        /// </summary>
        public bool Updating {
            get { return _updating; }
            set {
                _updating = value;
                if (_updating)
                    Force = false;
                OnPropertyChanged("Updating");
            }
        }

        /// <summary>
        ///     If true, the value set in the UI will be set to
        ///     this node every frame.
        /// </summary>
        public bool Force {
            get { return _force; }
            set {
                _force = value;
                if (_force)
                    Updating = false;
                OnPropertyChanged("Force");
            }
        }

        /// <summary>
        ///     If false, the "Cant write" icon won't be showed. This property is to be binded by WPF.
        /// </summary>
        public bool ShowCantWriteIcon { get { return !_hideCantWriteIcon && (!CanWrite); } }

        /// <summary>
        ///     Gets or sets whether the private children of this node are shown or not.
        /// </summary>
        public bool IsShowingPrivate {
            get { return _isShowingPrivate; }
            set {
                _isShowingPrivate = value;

                // Don't show private members of our own code.
                if (Type.Assembly != typeof(InspectorNode).Assembly)
                    Expand(true, _isShowingPrivate);
            }
        }

        /// <summary>
        ///     The target object (always represented by the root of the tree)
        ///     so it is looked up recursively.
        /// </summary>
        public Object Target {
            get {
                if (Parent == null)
                    return RootTarget;
                return Parent.Target;
            }
        }

        public InspectorNode Parent { get; private set; }

        /// <summary>
        ///     Used to reference this node in XAML
        /// </summary>
        public InspectorNode Itself { get { return this; } }

        /// <summary>
        ///     The name of the field this node represents.
        /// </summary>
        public String Name { get; private set; }

        /// <summary>
        ///     Name of this node shown in Inspector tree.
        /// </summary>
        public String FriendlyName {
            get {
                if (_friendlyName != null)
                    return _friendlyName;
                return Name;
            }
            set {
                _friendlyName = value;
                OnPropertyChanged("FriendlyName");
            }
        }

        /// <summary>
        ///     The type of the field this node represents.
        /// </summary>
        public Type Type {
            get { return _type; }
            private set {
                _type = value;
                OnPropertyChanged("Type");
            }
        }

        /// <summary>
        ///     A list that contains nodes that represent the
        ///     fields of the object represented by this node.
        /// </summary>
        public ObservableCollection<InspectorNode> Children { get; private set; }

        /// <summary>
        ///     The children as must be viewed by the UI layer.
        /// </summary>
        public CollectionViewSource ChildrenView {
            get { return _childrenView; }
            private set {
                _childrenView = value;
                OnPropertyChanged("ChildrenView");
            }
        }

        /// <summary>
        ///     A list that contains methods available for this object.
        ///     TODO: a global Dictionary(Type, List(Method)) might be better.
        /// </summary>
        public ICollection<InspectorNode> Methods { get; private set; }

        /// <summary>
        ///     True if the value this node represents can be set.
        /// </summary>
        public bool CanWrite { get { return Parent == null ? true : _setter != null; } }

        /// <summary>
        ///     True if the value this node represents can be get.
        /// </summary>
        public bool CanRead { get { return Parent == null ? true : _getter != null; } }

        /// <summary>
        ///     True if this node represents a property, otherwise
        ///     it represents a field. We store this information
        ///     in order to know when a ValueType is encapsulated
        ///     so we should instanciate it.
        /// </summary>
        public bool IsProperty { get; set; }

        /// <summary>
        ///     Gets whether this node is declared private inside its parent.
        /// </summary>
        public bool IsPrivate {
            get { return _isPrivate; }
            set {
                _isPrivate = value;
                OnPropertyChanged("IsPrivate");
            }
        }

        /// <summary>
        ///     True if Parent equals null.
        /// </summary>
        public bool IsRoot { get { return Parent == null; } }

        /// <summary>
        ///     True for nodes that will be auto-expanded when the UI
        ///     for it is generated. Setting this value will only have
        ///     effect if the UI hasn't been created.
        /// </summary>
        public bool AutoExpand { get; internal set; }

        /// <summary>
        ///     The object this node represents.
        ///     This will produce boxing/unboxing.
        /// </summary>
        public Object Property {
            set {
                try {
                    if (_setter != null) {
                        _setter(Target, value);

                        // Set the UserModified flag all the way to the root.
                        if (!UserModified) {
                            var n = this;
                            while (n != null) {
                                n.UserModified = true;
                                n = n.Parent;
                            }
                        }
                    }
                }
                catch (Exception e) {
                    // TODO: Change this behavior: instead disable the control and show the exception. Add a button to re-enable it.
                    GearsetResources.Console.Log("Gearset", "An error occured while setting the value of the property " + this + ". This value won't be inspectable anymore.");
                    GearsetResources.Console.Log("Gearset", "Exception Thrown:");
                    GearsetResources.Console.Log("Gearset", "   " + e.Message.Replace("\n", "\n   "));
                    _setter = null;
                }
            }
            get {
                try {
                    if (IsRoot)
                        return Target;
                    if (_getter != null)
                        return _getter(Target);

                    return null;
                }
                catch (Exception e) {
                    GearsetResources.Console.Log("Gearset", "An error occured while getting the value of the property " + this + ". This value won't be inspectable anymore.");
                    GearsetResources.Console.Log("Gearset", "Exception Thrown:");
                    GearsetResources.Console.Log("Gearset", "   " + e.Message.Replace("\n", "\n   "));
                    _getter = null;
                    return null;
                }
            }
        }

        /// <summary>
        ///     So we can notify when a bound property changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public void Expand() {
            Expand(false, false);
        }

        /// <summary>
        ///     Fills the list of Children with nodes.
        /// </summary>
        /// <param name="force">if set to <c>true</c> the children, if any, will be deleted and the node reexpanded.</param>
        public void Expand(bool force, bool includePrivate) {
            Dictionary<MemberInfo, InspectorReflectionHelper.SetterGetterPair> setterGetterDict;

            // Root will never be null, so we check if a child is null
            // or unreadable before trying to expanding it
            if (IsExtraNode)
                return;
            if (Parent != null && (!CanRead || Property == null))
                return;
            if (Children.Count != 0)
                if (force)
                    Children.Clear();
                else // Already expanded?
                    return;

            ChildrenView = new CollectionViewSource();
            ChildrenView.Source = Children;
            ChildrenView.Filter += CollectionViewSource_Filter;

            // If there's no UI container created for us yet then we can't expand
            if (UiContainer == null)
                return;

            try {
                setterGetterDict = InspectorReflectionHelper.GetSetterGetterDict3(this);
            }
            catch (CompileErrorException) {
                GearsetResources.Console.Log("Gearset", "A compiler error occured, try verifying that the sealed class you're inspecting is private");
                return;
            }

            var fields = new List<FieldInfo>(Type.GetInstanceFields(!includePrivate));
            var properties = new List<PropertyInfo>(Type.GetInstanceProperties(!includePrivate));
            var methods = new List<MethodInfo>(Type.GetInstanceMethods());
            var sortedChildren = new List<InspectorNode>(fields.Count + properties.Count);

            InspectorReflectionHelper.SetterGetterPair pair;

            foreach (var field in fields) {
                if (field.GetCustomAttributes(typeof(InspectorIgnoreAttribute), true).Length > 0)
                    continue;
                if (field.FieldType.IsPointer)
                    continue;
                try {
                    pair = setterGetterDict[field];
                }
                catch {
                    GearsetResources.Console.Log("Gearset", "Field {0} could not be inspected.", field.Name);
                    continue;
                }

                // Do we have a friendly name?
                var friendlyName = field.Name;
                var hideCantWriteIcon = false;
                foreach (InspectorAttribute attribute in field.GetCustomAttributes(typeof(InspectorAttribute), true)) {
                    if (attribute.FriendlyName != null)
                        friendlyName = attribute.FriendlyName;
                    hideCantWriteIcon = attribute.HideCantWriteIcon;
                }
                sortedChildren.Add(new InspectorNode(this, field.FieldType, field.Name, pair.Setter, pair.Getter, hideCantWriteIcon) {
                    IsProperty = false,
                    FriendlyName = friendlyName,
                    IsPrivate = field.IsPrivate || field.IsFamilyOrAssembly || field.IsFamily || field.IsAssembly || field.IsFamilyAndAssembly
                });
            }

            foreach (var property in properties) {
                if (property.GetCustomAttributes(typeof(InspectorIgnoreAttribute), true).Length > 0)
                    continue;
                if (property.PropertyType.IsPointer)
                    continue;
                try {
                    pair = setterGetterDict[property];
                }
                catch {
                    GearsetResources.Console.Log("Gearset", "Property {0} could not be inspected.", property.Name);
                    continue;
                }

                // Do we have a friendly name?
                var friendlyName = property.Name;
                var hideCantWriteIcon = false;
                foreach (InspectorAttribute attribute in property.GetCustomAttributes(typeof(InspectorAttribute), true)) {
                    if (attribute.FriendlyName != null)
                        friendlyName = attribute.FriendlyName;
                    hideCantWriteIcon = attribute.HideCantWriteIcon;
                }

                var getMethod = property.GetGetMethod(true);
                var setMethod = property.GetSetMethod(true);
                var privateGet = getMethod != null ? getMethod.IsPrivate || getMethod.IsFamilyOrAssembly || getMethod.IsFamily || getMethod.IsAssembly || getMethod.IsFamilyAndAssembly : false;
                var privateSet = setMethod != null ? setMethod.IsPrivate || setMethod.IsFamilyOrAssembly || setMethod.IsFamily || setMethod.IsAssembly || setMethod.IsFamilyAndAssembly : false;

                // If there's one that's not private, add it in the public part.
                if ((!privateGet && getMethod != null) || (!privateSet && setMethod != null)) {
                    sortedChildren.Add(new InspectorNode(this,
                        property.PropertyType,
                        property.Name,
                        privateSet ? null : pair.Setter,
                        privateGet ? null : pair.Getter,
                        hideCantWriteIcon) {
                            IsProperty = true,
                            FriendlyName = friendlyName,
                            IsPrivate = false
                        });
                }

                // If on accessor is private, we have to add it again in the private part with full access.
                if (includePrivate && (privateGet || privateSet)) {
                    sortedChildren.Add(new InspectorNode(this,
                        property.PropertyType,
                        property.Name,
                        pair.Setter,
                        pair.Getter,
                        hideCantWriteIcon) {
                            IsProperty = true,
                            FriendlyName = friendlyName,
                            IsPrivate = true
                        });
                }
            }

            // HACK: this could be done in the UI layer using the ListView.View property.
            //sortedChildren.Sort(AlphabeticalComparison);
            foreach (var child in sortedChildren)
                Children.Add(child);

            // Special markers to add children to special types
            // TODO: make this extensible.
            // EXTRAS:
            if (typeof(IEnumerable).IsAssignableFrom(Type))
                Children.Add(new InspectorNode(this, typeof(CollectionMarker), String.Empty, null, _getter != null ? _getter : x => { return RootTarget; }, false) { IsExtraNode = true });
            if (typeof(Texture2D).IsAssignableFrom(Type))
                Children.Add(new InspectorNode(this, typeof(Texture2DMarker), String.Empty, null, _getter != null ? _getter : x => { return RootTarget; }, false) { IsExtraNode = true });
            if (typeof(Vector2).IsAssignableFrom(Type))
                Children.Add(new InspectorNode(this, typeof(float), "Vector Length",
                    null, // Setter
                    x => ((Vector2)Property).Length(), true) // Getter
                { IsExtraNode = true });
            if (typeof(Vector3).IsAssignableFrom(Type))
                Children.Add(new InspectorNode(this, typeof(float), "Vector Length",
                    null, // Setter
                    x => ((Vector3)Property).Length(), true) // Getter
                { IsExtraNode = true });

            // Add methods that don't take any params
            Methods.Clear();
            foreach (var method in methods) {
                if (method.GetParameters().Length != 0)
                    continue;
                if (method.IsDefined(typeof(CompilerGeneratedAttribute), true))
                    continue;
                if (method.IsSpecialName)
                    continue;
                if (method.DeclaringType == typeof(Object))
                    continue;

                // Do we have a friendly name?
                var friendlyName = method.Name;
                foreach (InspectorMethodAttribute attribute in method.GetCustomAttributes(typeof(InspectorMethodAttribute), true)) {
                    if (attribute.FriendlyName != null) {
                        friendlyName = attribute.FriendlyName;
                        var methodNodec = new InspectorNode(this, typeof(void), method.Name, null, null, false) { FriendlyName = friendlyName };
                        methodNodec.Method = method;
                        Children.Add(methodNodec);
                    }
                }
                var methodNode = new InspectorNode(this, typeof(void), method.Name, null, null, false) { FriendlyName = friendlyName };
                methodNode.Method = method;
                Methods.Add(methodNode);
            }

            // Add extension methods (if any)
            foreach (var t in ExtensionMethodTypes) {
                foreach (var method in t.GetStaticMethods()) {
                    if (method.GetParameters().Length != 1)
                        continue;
                    // Do we have a friendly name for the method?
                    var friendlyName = method.Name;
                    foreach (InspectorMethodAttribute attribute in method.GetCustomAttributes(typeof(InspectorMethodAttribute), true)) {
                        if (attribute.FriendlyName != null) {
                            friendlyName = attribute.FriendlyName;
                            var methodNodec = new InspectorNode(this, typeof(void), method.Name, null, null, false) { FriendlyName = friendlyName };
                            methodNodec.Method = method;
                            Children.Add(methodNodec);
                        }
                    }
                    var paramType = method.GetParameters()[0].ParameterType;
                    if (paramType.IsAssignableFrom(Type)) {
                        var methodNode = new InspectorNode(this, typeof(void), method.Name, null, null, true) { FriendlyName = friendlyName };
                        methodNode.Method = method;
                        Methods.Add(methodNode);
                    }
                }
            }
        }

        static void CollectionViewSource_Filter(object sender, FilterEventArgs e) {
            e.Accepted = GearsetResources.Console.Inspector.FilterPredicate(e.Item);
        }

        static int AlphabeticalComparison(InspectorNode a, InspectorNode b) {
            return String.Compare(a.Name, b.Name);
        }

        /// <summary>
        ///     Get the TreeViewItems (containers) and let the InspectorTreeNodes
        ///     know where they are.
        /// </summary>
        void ItemContainerGenerator_StatusChanged(object sender, EventArgs e) {
            if (UiContainer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated) {
                var i = 0;
                foreach (var item in Children) {
                    var child = item;
                    if (child.UiContainer == null || (child.UiContainer != null && child.UiContainer.Header != null && child.UiContainer.Header.ToString().Equals("{DisconnectedItem}"))) {
                        child.UiContainer = (TreeViewItem)UiContainer.ItemContainerGenerator.ContainerFromItem(child);

                        if (GearsetResources.Console.Inspector.Config.ModifiedOnly && child.UiContainer != null)
                            child.UiContainer.IsExpanded = true;
                    }
                    i++;
                }
            }
        }

        /// <summary>
        ///     Returns the path that leads to this node from the
        ///     Target object with and added point at the end.
        /// </summary>
        internal String GetPath() {
            if (Parent == null)
                return Name;
            return Parent.GetPath() + "." + Name;
        }

        public override string ToString() {
            return GetPath();
        }

        /// <summary>
        ///     Method to rise the event.
        /// </summary>
        void OnPropertyChanged(string name) {
            var handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        ///     Calls the update callback so the UI can get updated
        ///     and recursively call Update on it's children.
        /// </summary>
        public void Update() {
            if (UiContainer == null)
                return;
            var visuallyExpanded = UiContainer.IsExpanded;

            // Check if property suddenly became null.
            if (CanRead && !IsRoot && Property == null) {
                Children.Clear();
            }

            // Update if the update button is pressed and if we're expanded (vissually) and if there's a getter
            //if (VisualItem != null && Updating && (!visuallyExpanded || VisualItem.UpdateIfExpanded) && CanRead)
            if (VisualItem != null && Updating && CanRead)
                VisualItem.UpdateUi(Property);

            // It makes no sense to force the value while updating.
            if (!Updating && Force)
                VisualItem.UpdateVariable();


            // Only update children if we're visually expanded.
            if (UiContainer != null && visuallyExpanded) {
                if (ChildrenView != null && ChildrenView.View != null) {
                    foreach (InspectorNode child in ChildrenView.View) {
                        child.Update();
                    }
                }
            }
        }
    }
}
