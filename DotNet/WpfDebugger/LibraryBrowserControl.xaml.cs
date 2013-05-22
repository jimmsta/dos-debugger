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
using Disassembler2;
using Disassembler2.Omf;
using Util.Windows.Controls;

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
                //MessageBox.Show("Show info about " + sender.ToString());
                var x = (LibraryBrowserViewModel.SymbolItem)sender;
                if (x.Symbol.BaseSegment != null &&
                    x.Symbol.BaseSegment.Class.EndsWith("CODE"))
                {
                    DisassembleSegment(x.Symbol.BaseSegment, (int)x.Symbol.Offset);
                }
            }
        }

        private void DisassembleSegment(LogicalSegment segment, int entryIndex)
        {
            ImageChunk image = segment.Image;
            //Disassembler16New dasm=new Disassembler16New(
            //Disassembler16New .Disassemble(image);
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
                else if (sender is LibraryBrowserViewModel.SymbolAliasItem)
                    obj = ((LibraryBrowserViewModel.SymbolAliasItem)sender).Alias;

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
                        from ObjectModule module in library.Modules
                        orderby module.ObjectName
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
            public ObservableCollection<ITreeNode> Symbols { get; private set; }

            public ModuleItem(ObjectModule module)
            {
                if (module == null)
                    throw new ArgumentNullException("module");

                this.Module = module;
                this.Symbols =
                    new ObservableCollection<ITreeNode>(
                        (from alias in module.Aliases
                         select (ITreeNode)new SymbolAliasItem(alias)
                        ).Concat(
                         from symbol in module.DefinedNames
                         let segName = (symbol.BaseSegment != null) ?
                                       symbol.BaseSegment.Name : null
                         orderby segName, symbol.Offset, symbol.Name
                         select (ITreeNode)new SymbolItem(symbol)
                        ));
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
            public DefinedSymbol Symbol { get; private set; }

            public SymbolItem(DefinedSymbol symbol)
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
                        Symbol.Name, Symbol.BaseSegment.Name, Symbol.Offset);
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

                    string className = Symbol.BaseSegment.Class;
                    if (className.EndsWith("CODE"))
                    {
                        if (Symbol.Scope == SymbolScope.Private)
                            return "LocalProcedureImage";
                        else
                            return "ProcedureImage";
                    }
                    else if (className.EndsWith("DATA"))
                    {
                        if (Symbol.Scope == SymbolScope.Private)
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

        internal class SymbolAliasItem : ITreeNode
        {
            public SymbolAlias Alias { get; private set; }

            public SymbolAliasItem(SymbolAlias alias)
            {
                if (alias == null)
                    throw new ArgumentNullException("alias");
                this.Alias = alias;
            }

            public override string ToString()
            {
                return string.Format("{0} -> {1}",
                    Alias.Name, Alias.SubstituteName);
            }

            public string Text
            {
                get { return this.ToString(); }
            }

            public string ImageKey
            {
                get { return "ProcedureAliasImage"; }
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
