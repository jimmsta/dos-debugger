using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Disassembler;
using Disassembler.Omf;

namespace WpfDebugger
{
    /// <summary>
    /// Interaction logic for LibraryBrowserControl.xaml
    /// </summary>
    public partial class LibraryBrowserControl : UserControl
    {
        public LibraryBrowserControl()
        {
            InitializeComponent();
#if false
        typeof(VirtualizingStackPanel).GetProperty("IsPixelBased", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this, true, null);
#endif
        }

        private ObjectLibrary library;

        public ObjectLibrary Library
        {
            get { return library; }
            set
            {
                library = value;
                if (library == null)
                {
                    this.DataContext = null;
                }
                else
                {
                    var viewModel = new LibraryBrowserViewModel(library);
                    this.DataContext = viewModel;
                    myTreeView.ItemsSource = viewModel.Libraries;
                }
            }
        }

        private void TreeView_ItemActivate(object sender, EventArgs e)
        {
            if (sender is LibraryBrowserViewModel.LibraryItem)
            {
            }
            else if (sender is LibraryBrowserViewModel.ModuleItem)
            {
            }
            else if (sender is LibraryBrowserViewModel.SymbolItem)
            {
                MessageBox.Show("Show info about " + sender.ToString());
            }
        }

        private void TreeView_SelectionChanged(object sender, EventArgs e)
        {
            if (RequestProperty != null)
            {
                object obj = null;
                if (sender is LibraryBrowserViewModel.LibraryItem)
                    obj = ((LibraryBrowserViewModel.LibraryItem)sender).Library;
                else if (sender is LibraryBrowserViewModel.ModuleItem)
                    obj = ((LibraryBrowserViewModel.ModuleItem)sender).Module;
                else if (sender is LibraryBrowserViewModel.SymbolItem)
                    obj = ((LibraryBrowserViewModel.SymbolItem)sender).Symbol;

                if (obj != null)
                    RequestProperty(this, new RequestPropertyEventArgs(obj));
            }
        }

        public event EventHandler<RequestPropertyEventArgs> RequestProperty;
    }

    public class RequestPropertyEventArgs : EventArgs
    {
        public object SelectedObject { get; private set; }
        public RequestPropertyEventArgs(object selectedObject)
        {
            this.SelectedObject = selectedObject;
        }
    }

    internal class LibraryBrowserViewModel
    {
        public LibraryItem[] Libraries { get; private set; }
        public LibraryItem Library { get { return Libraries[0]; } }

        public LibraryBrowserViewModel(ObjectLibrary library)
        {
            this.Libraries = new LibraryItem[1] { new LibraryItem(library) };    
        }

        internal class LibraryItem : ITreeNode
        {
            [Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ExpandableObject]
            public ObjectLibrary Library { get; private set; }
            public ObservableCollection<ModuleItem> Modules { get; private set; }
            public string Name { get { return "Library"; } }
            
            public LibraryItem(ObjectLibrary library)
            {
                if (library == null)
                    throw new ArgumentNullException("library");

                this.Library = library;
                this.Modules = 
                    new ObservableCollection<ModuleItem>(
                        from module in library.Modules
                        select new ModuleItem(module));
            }

            public string Text
            {
                get { return "Library"; }
            }

            public string ImageKey
            {
                get { return "LibraryImage"; }
            }

            public bool HasChildren
            {
                get { return Modules.Count > 0; }
            }

            public IEnumerable<ITreeNode> GetChildren()
            {
                return Modules;
            }
        }

        internal class ModuleItem : ITreeNode
        {
            [Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ExpandableObject]
            public ObjectModule Module { get; private set; }
            public string Name { get { return Module.ObjectName; } }
            public ObservableCollection<SymbolItem> Symbols { get; private set; }

            public ModuleItem(ObjectModule module)
            {
                if (module == null)
                    throw new ArgumentNullException("module");

                this.Module = module;
                this.Symbols = 
                    new ObservableCollection<SymbolItem>(
                        from publicName in module.PublicNames
                        let segName = (publicName.BaseSegment == null)? 
                                      "" : publicName.BaseSegment.SegmentName
                        orderby segName, publicName.Offset, publicName.Name
                        select new SymbolItem(publicName));
            }

            public string Text
            {
                get { return this.Name; }
            }

            public string ImageKey
            {
                get { return "ModuleImage"; }
            }

            public bool HasChildren
            {
                get { return Symbols.Count > 0; }
            }

            public IEnumerable<ITreeNode> GetChildren()
            {
                return Symbols;
            }
        }

        internal class SymbolItem : ITreeNode
        {
            [Xceed.Wpf.Toolkit.PropertyGrid.Attributes.ExpandableObject]
            public PublicNameDefinition Symbol { get; private set; }

            public SymbolItem(PublicNameDefinition symbol)
            {
                if (symbol == null)
                    throw new ArgumentNullException("symbol");
                this.Symbol = symbol;
            }

            public override string ToString()
            {
                if (Symbol.BaseSegment == null)
                {
                    return string.Format("{1:X4}:{2:X4}  {0}",
                        Symbol.Name, Symbol.BaseFrame, Symbol.Offset);
                }
                else
                {
                    return string.Format("{1}+{2:X4}  {0}",
                        Symbol.Name, Symbol.BaseSegment.SegmentName, Symbol.Offset);
                }
            }

            public string Text
            {
                get { return this.ToString(); }
            }

            public string ImageKey
            {
                get
                {
                    if (Symbol.BaseSegment == null)
                    {
                        // An absolute symbol is typically used to store
                        // a constant.
                        return "ConstantImage";
                    }

                    string className = Symbol.BaseSegment.ClassName;
                    if (className.EndsWith("CODE"))
                    {
                        if (Symbol.IsLocal)
                            return "LocalProcedureImage";
                        else
                            return "ProcedureImage";
                    }
                    else if (className.EndsWith("DATA"))
                    {
                        if (Symbol.IsLocal)
                            return "LocalFieldImage";
                        else
                            return "FieldImage";
                    }
                    else
                        return null;
                }
            }

            public bool HasChildren
            {
                get { return false; }
            }

            public IEnumerable<ITreeNode> GetChildren()
            {
                return null;
            }
        }
    }
}
