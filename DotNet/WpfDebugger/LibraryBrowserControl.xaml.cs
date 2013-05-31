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
            object obj = GetObjectFromItem(sender);
            if (obj != null)
                ActivateObject(obj);
        }

        private void ActivateObject(object obj)
        {
            if (obj is LogicalSegment)
            {
                DisassembleSegment((LogicalSegment)obj, 0);
            }
            else if (obj is DefinedSymbol)
            {
                DefinedSymbol symbol = (DefinedSymbol)obj;
                if (symbol.BaseSegment != null &&
                    symbol.BaseSegment.Class.EndsWith("CODE"))
                {
                    DisassembleSegment(symbol.BaseSegment, (int)symbol.Offset);
                }
            }
#if false
            else if (sender is LibraryBrowserViewModel.ModuleItem)
            {
                // Disassemble the first code segment.
                var module = ((LibraryBrowserViewModel.ModuleItem)sender).Module;
                LogicalSegment segment = module.Segments.FirstOrDefault(
                    s => s.Class.EndsWith("CODE"));
                if (segment != null)
                {
                    DisassembleSegment(segment, module);
                }
            }
#endif
        }

        private void DisassembleSegment(LogicalSegment segment, int offset)
        {
            // Raise request navigate event.
            if (this.RequestNavigate != null)
            {
                AssemblyUri uri = new AssemblyUri(library, segment, offset);
                var e = new RequestNavigateEventArgs(uri, null);
                this.RequestNavigate(this, e);
            }
        }

        private void TreeView_SelectionChanged(object sender, EventArgs e)
        {
            object obj = GetObjectFromItem(sender);

            if (RequestProperty != null && obj != null)
                RequestProperty(this, new RequestPropertyEventArgs(obj));

            if (obj != null)
                ActivateObject(obj);
        }

        private static object GetObjectFromItem(object sender)
        {
            object obj;
            if (sender is LibraryBrowserViewModel.LibraryItem)
                obj = ((LibraryBrowserViewModel.LibraryItem)sender).Library;
            else if (sender is LibraryBrowserViewModel.ModuleItem)
                obj = ((LibraryBrowserViewModel.ModuleItem)sender).Module;
            else if (sender is LibraryBrowserViewModel.SymbolItem)
                obj = ((LibraryBrowserViewModel.SymbolItem)sender).Symbol;
            else if (sender is LibraryBrowserViewModel.SymbolAliasItem)
                obj = ((LibraryBrowserViewModel.SymbolAliasItem)sender).Alias;
            else if (sender is LibraryBrowserViewModel.SegmentItem)
                obj = ((LibraryBrowserViewModel.SegmentItem)sender).Segment;
            else
                obj = null;
            return obj;
        }

        public event EventHandler<RequestPropertyEventArgs> RequestProperty;
        public event EventHandler<RequestNavigateEventArgs> RequestNavigate;
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
                        orderby module.Name
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
            public ObjectModule Module { get; private set; }
            public string Name
            {
                get
                {
                    if (Module.Name == null)
                        return "(" + Module.SourceName + ")";
                    else
                        return Module.Name;
                }
            }
            public List<ITreeNode> Symbols { get; private set; }

            public ModuleItem(ObjectModule module)
            {
                if (module == null)
                    throw new ArgumentNullException("module");

                this.Module = module;
                this.Symbols = new List<ITreeNode>();

                ConstantSegmentItem constantGroup = new ConstantSegmentItem(module);
                if (constantGroup.HasChildren)
                    this.Symbols.Add(constantGroup);

                // Hide zero-length segments.
                this.Symbols.AddRange(
                    from segment in module.Segments
                    where segment.Length > 0
                    orderby segment.Class, segment.Name
                    select new SegmentItem(segment, module));

                this.Symbols.AddRange(
                    from alias in module.Aliases
                    select (ITreeNode)new SymbolAliasItem(alias));

#if false
                this.Symbols.AddRange(
                    from symbol in module.DefinedNames
                    where symbol.BaseSegment != null
                    orderby symbol.BaseSegment.Name, symbol.Offset, symbol.Name
                    select new SymbolItem(symbol));
#endif
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

        internal class ConstantSegmentItem : ITreeNode
        {
            ObjectModule module;
            List<SymbolItem> constants;

            public ConstantSegmentItem(ObjectModule module)
            {
                this.module = module;

                // Find all defined names with absolute segment. These
                // symbols are most likely constants.
                this.constants = new List<SymbolItem>(
                    from symbol in module.DefinedNames
                    where symbol.BaseSegment == null
                    orderby symbol.Name
                    select new SymbolItem(symbol));
            }

            public string Text
            {
                get { return "Constants"; }
            }

            public string ImageKey
            {
                get { return "SegmentImage"; }
            }

            public bool HasChildren
            {
                get { return constants.Count > 0; }
            }

            public IEnumerable<ITreeNode> GetChildren()
            {
                return constants;
            }
        }

        internal class SegmentItem : ITreeNode
        {
            readonly LogicalSegment segment;
            readonly List<SymbolItem> symbols;

            public SegmentItem(LogicalSegment segment, ObjectModule module)
            {
                this.segment = segment;
                this.symbols = new List<SymbolItem>(
                    from symbol in module.DefinedNames
                    where symbol.BaseSegment == segment
                    orderby symbol.Offset, symbol.Name
                    select new SymbolItem(symbol));
            }

            public LogicalSegment Segment
            {
                get { return segment; }
            }

            public string Text
            {
                get
                {
                    return string.Format("{1}: {0} [{2}]",
                        segment.Name, segment.Class, segment.Length);
                }
            }

            public string ImageKey
            {
                get
                {
                    // if (segment.Combination == Private), return private;
                    return "SegmentImage";
                }
            }

            public bool HasChildren
            {
                get { return symbols.Count > 0; }
            }

            public IEnumerable<ITreeNode> GetChildren()
            {
                return symbols;
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
