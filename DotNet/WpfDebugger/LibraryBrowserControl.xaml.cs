using System;
using System.Collections.Generic;
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
        }

        private ObjectLibrary library;

        public ObjectLibrary Library
        {
            get { return library; }
            set
            {
                library = value;
                if (library == null)
                    this.DataContext = null;
                else
                    this.DataContext = new LibraryBrowserViewModel(library);
            }
        }
    }

    internal class LibraryBrowserViewModel
    {
        public LibraryItem[] Libraries { get; private set; }

        public LibraryBrowserViewModel(ObjectLibrary library)
        {
            this.Libraries = new LibraryItem[1] { new LibraryItem(library) };    
        }

        internal class TreeViewItemBase : INotifyPropertyChanged
        {
            private bool isExpanded;
            private bool isSelected;

            public bool IsExpanded
            {
                get { return isExpanded; }
                set
                {
                    if (isExpanded != value)
                    {
                        isExpanded = value;
                        //RaisePropertyChanged("IsExpanded");
                    }
                }
            }

            public bool IsSelected
            {
                get { return isSelected; }
                set
                {
                    if (isSelected != value)
                    {
                        isSelected = value;
                        //RaisePropertyChanged("IsSelected");
                    }
                }
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

        internal class LibraryItem : TreeViewItemBase
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
                        from module in library.Modules
                        select new ModuleItem(module));
            }
        }

        internal class ModuleItem : TreeViewItemBase
        {
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
                        select new SymbolItem(publicName));
            }
        }

        internal class SymbolItem : TreeViewItemBase
        {
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
                    return string.Format("{0} : {1:X4}:{2:X4}",
                        Symbol.Name, Symbol.BaseFrame, Symbol.Offset);
                }
                else
                {
                    return string.Format("{0} : {1}+{2:X}h",
                        Symbol.Name, Symbol.BaseSegment.SegmentName, Symbol.Offset);
                }
            }
        }
    }
}
