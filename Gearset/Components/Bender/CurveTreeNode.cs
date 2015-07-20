using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Gearset.Components {
    public class CurveTreeNode : INotifyPropertyChanged {
        protected bool areParentsVisible;
        protected bool isVisible;
        bool _isExpanded;
        bool _isSelected;
        ObservableCollection<CurveTreeNode> _children;

        public CurveTreeNode(String name, CurveTreeNode parent)
            : this(parent) {
            Name = name;
        }

        /// <summary>
        /// Only to be used by subclasses that override the name property.
        /// </summary>
        protected CurveTreeNode(CurveTreeNode parent) {
            Parent = parent;
            Children = new ObservableCollection<CurveTreeNode>();
            isVisible = true;
            areParentsVisible = true;
        }

        public virtual String Name { get; set; }

        public virtual bool AreParentsVisible {
            get { return areParentsVisible; }
            set {
                var prevValue = areParentsVisible;
                areParentsVisible = value;
                if (prevValue != areParentsVisible) {
                    foreach (var child in Children) {
                        child.AreParentsVisible = areParentsVisible && isVisible;
                    }
                    OnPropertyChanged("AreParentsVisible");
                }
            }
        }

        public virtual bool IsVisible {
            get { return isVisible; }
            set {
                isVisible = value;
                foreach (var child in Children) {
                    child.AreParentsVisible = areParentsVisible && isVisible;
                }
                OnPropertyChanged("IsVisible");
            }
        }

        public bool IsExpanded {
            get { return _isExpanded; }
            set {
                if (_isExpanded != value) {
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
                if (_isExpanded && Parent != null)
                    Parent.IsExpanded = true;
            }
        }

        public bool IsSelected {
            get { return _isSelected; }
            set {
                _isSelected = value;
                if (_isSelected)
                    IsExpanded = true;
                OnPropertyChanged("IsSelected");
            }
        }

        public CurveTreeNode Parent { get; private set; }

        public ObservableCollection<CurveTreeNode> Children {
            get { return _children; }
            set {
                _children = value;
                OnPropertyChanged("Children");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
