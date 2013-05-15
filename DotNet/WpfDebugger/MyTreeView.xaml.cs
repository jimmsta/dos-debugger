using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;

namespace WpfDebugger
{
    /// <summary>
    /// Interaction logic for MyTreeView.xaml
    /// </summary>
    public partial class MyTreeView : UserControl
    {
        public MyTreeView()
        {
            InitializeComponent();
        }

        MyTreeViewModel viewModel;

        public IEnumerable<ITreeNode> ItemsSource
        {
            set
            {
                this.viewModel = new MyTreeViewModel(value, this);
                this.treeView.ItemsSource = viewModel.Items;
            }
        }

        private void treeView_KeyDown(object sender, KeyEventArgs e)
        {
            if (!(sender is ListBoxItem))
                return;

            MyTreeViewItem item = ((ListBoxItem)sender).DataContext as MyTreeViewItem;
            if (item == null)
                return;

            if (e.Key == Key.Left)
            {
                // If the item is expanded, collapse it; otherwise, 
                // navigate to its parent.
                if (item.IsExpanded)
                {
                    item.IsExpanded = false;
                }
                else
                {
                    MyTreeViewItem parent = item.Parent;
                    if (parent != null)
                    {
                        parent.IsSelected = true;
                        treeView.ScrollIntoView(parent);
                        var container = treeView.ItemContainerGenerator.ContainerFromItem(parent)
                            as ListBoxItem;
                        if (container != null)
                            container.Focus();
                    }
                }
            }
            else if (e.Key == Key.Right)
            {
                // If the item can be expanded but is not expanded, expand
                // it; if the item is already expanded, go to the first
                // child.
                if (item.IsExpanded)
                {
                    MyTreeViewItem child = item.FirstChild;
                    if (child != null)
                    {
                        child.IsSelected = true;
                        treeView.ScrollIntoView(child);
                        var container = treeView.ItemContainerGenerator.ContainerFromItem(child)
                            as ListBoxItem;
                        if (container != null)
                            container.Focus();
                    }
                }
                else if (item.HasChildren)
                {
                    item.IsExpanded = true;
                }
            }
        }

        private void treeView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is ListBoxItem))
                return;

            MyTreeViewItem item = ((ListBoxItem)sender).DataContext as MyTreeViewItem;
            if (item == null)
                return;

            // If this item is expandable, toggle its expanding status.
            if (item.HasChildren)
            {
                item.IsExpanded = !item.IsExpanded;
            }

            // Raise an ItemActivate event, as a WinForms tree view would.
            if (ItemActivate != null)
            {
                ItemActivate(item.Node, null);
            }
        }

        public void RaiseSelectionChanged(object sender, EventArgs e)
        {
            if (SelectionChanged != null)
                SelectionChanged(sender, e);
        }

        public event EventHandler ItemActivate;
        public event EventHandler SelectionChanged;
    }

    public interface ITreeNode
    {
        /// <summary>
        /// Gets the text to display for the tree node.
        /// </summary>
        string Text { get; }

        /// <summary>
        /// Gets a key that points to the StaticResource that contains the
        /// image to display for the node; null if none.
        /// </summary>
        string ImageKey { get; }

        /// <summary>
        /// Gets a flag that indicates whether this node has any child.
        /// </summary>
        bool HasChildren { get; }

        /// <summary>
        /// Gets a collection of child nodes.
        /// </summary>
        IEnumerable<ITreeNode> GetChildren();
    }

    internal class MyTreeViewModel
    {
        public ObservableCollection<MyTreeViewItem> Items { get; private set; }
        public MyTreeView Visual { get; private set; }

        public MyTreeViewModel(IEnumerable<ITreeNode> nodes, MyTreeView visual)
        {
            this.Visual = visual;
            this.Items =
                new ObservableCollection<MyTreeViewItem>(
                    from node in nodes
                    select new MyTreeViewItem(node, 0, this));
        }

        internal readonly HashSet<ITreeNode> SelectedNodes = new HashSet<ITreeNode>();
        internal readonly HashSet<ITreeNode> ExpandedNodes = new HashSet<ITreeNode>();
    }

    internal class MyTreeViewItem : INotifyPropertyChanged
    {
        private readonly MyTreeViewModel model;
        private readonly ITreeNode node;
        private readonly int level;
        private bool isExpanded;
        private bool isSelected;

        public MyTreeViewItem(ITreeNode node, int level, MyTreeViewModel model)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            if (model == null)
                throw new ArgumentNullException("model");
            if (level < 0)
                throw new ArgumentOutOfRangeException("level");

            this.node = node;
            this.model = model;
            this.level = level;
            this.isExpanded = model.ExpandedNodes.Contains(node);
            this.isSelected = model.SelectedNodes.Contains(node);
        }

        /// <summary>
        /// Gets the indention level of this node; 0 indicates root.
        /// </summary>
        public int Level { get { return level; } }

        /// <summary>
        /// Gets the underlying node.
        /// </summary>
        public ITreeNode Node { get { return node; } }

        /// <summary>
        /// Gets the parent of this node.
        /// </summary>
        public MyTreeViewItem Parent
        {
            get
            {
                int index = model.Items.IndexOf(this);
                while (--index >= 0 && model.Items[index].level >= this.level)
                    continue;
                if (index < 0)
                    return null;
                else
                    return model.Items[index];
            }
        }

        /// <summary>
        /// Gets the first child of this node.
        /// </summary>
        public MyTreeViewItem FirstChild
        {
            get
            {
                int index = model.Items.IndexOf(this) + 1;
                if (index < model.Items.Count && model.Items[index].Level > this.level)
                    return model.Items[index];
                else
                    return null;
            }
        }

        /// <summary>
        /// Gets or sets whether this tree node is expanded in the UI.
        /// </summary>
        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                if (isExpanded == value)
                    return;

                isExpanded = value;
                if (isExpanded)
                {
                    model.ExpandedNodes.Add(node);
                    ExpandChildren();
                }
                else
                {
                    model.ExpandedNodes.Remove(node);
                    CollapseChildren();
                }
                RaisePropertyChanged("IsExpanded");
            }
        }

        private int ExpandChildren()
        {
            int index = model.Items.IndexOf(this);
            int count = 0;
            foreach (var child in node.GetChildren())
            {
                ++count;
                MyTreeViewItem item = new MyTreeViewItem(child, Level + 1, model);
                model.Items.Insert(index + count, item);
                if (item.IsExpanded)
                {
                    count += item.ExpandChildren();
                }
            }
            return count;
        }

        private void CollapseChildren()
        {
            int index = model.Items.IndexOf(this) + 1;
            while (index < model.Items.Count && model.Items[index].Level > this.Level)
            {
                model.Items[index].IsSelected = false;
                model.Items.RemoveAt(index);
            }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set
            {
                if (IsSelected == value)
                    return;

                isSelected = value;
                if (value)
                {
                    model.SelectedNodes.Add(node);
                    model.Visual.RaiseSelectionChanged(this.node, null);
                }
                else
                {
                    model.SelectedNodes.Remove(node);
                }
                RaisePropertyChanged("IsSelected");
            }
        }

        public string Text
        {
            get { return node.Text; }
        }

        public Visibility ImageVisibility
        {
            get
            {
                if (node.ImageKey != null)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public ImageSource Image
        {
            get
            {
                string imageKey = node.ImageKey;
                if (imageKey != null)
                    return (ImageSource)Application.Current.FindResource(imageKey);
                else
                    return null;
            }
        }

        public bool HasChildren
        {
            get { return node.HasChildren; }
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, e);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    internal class ConvertLevelToIndent : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (int)value * 16;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
