using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Data;

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
    }

    public interface ITreeNode
    {
        string Text { get; }
        bool HasChildren { get; }
        IEnumerable<ITreeNode> GetChildren();
    }

    public class MyTreeViewModel
    {
        public ObservableCollection<MyTreeViewItem> Items { get; private set; }

        public MyTreeViewModel(IEnumerable<ITreeNode> nodes)
        {
            this.Items =
                new ObservableCollection<MyTreeViewItem>(
                    from node in nodes
                    select new MyTreeViewItem(node, 0, this));
        }

        internal ITreeNode SelectedNode { get; set; }
        internal readonly HashSet<ITreeNode> ExpandedNodes = new HashSet<ITreeNode>();
    }

    public class MyTreeViewItem
    {
        private MyTreeViewModel model;
        private bool isExpanded;

        public int Level { get; private set; }

        public ITreeNode Node { get; private set; }

        public MyTreeViewItem(ITreeNode node, int level, MyTreeViewModel model)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            if (model == null)
                throw new ArgumentNullException("model");
            if (level < 0)
                throw new ArgumentOutOfRangeException("level");

            this.Node = node;
            this.model = model;
            this.Level = level;
            this.isExpanded = model.ExpandedNodes.Contains(node);
        }

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
                    model.ExpandedNodes.Add(Node);
                    ExpandChildren();
                }
                else
                {
                    model.ExpandedNodes.Remove(Node);
                    CollapseChildren();
                }
            }
        }

        private int ExpandChildren()
        {
            int index = model.Items.IndexOf(this);
            int count = 0;
            foreach (var child in Node.GetChildren())
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
                model.Items.RemoveAt(index);
            }
        }

        public bool IsSelected
        {
            get { return model.SelectedNode == Node; }
            set
            {
                if (IsSelected == value)
                    return;

                model.SelectedNode = Node;
            }
        }

        public string Text
        {
            get { return Node.Text; }
        }

        public bool HasChildren
        {
            get { return Node.HasChildren; }
        }
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
