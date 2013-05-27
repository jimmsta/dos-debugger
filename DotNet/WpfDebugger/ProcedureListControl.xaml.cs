using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Navigation;
using Disassembler;

namespace WpfDebugger
{
    /// <summary>
    /// Interaction logic for ProcedureListControl.xaml
    /// </summary>
    public partial class ProcedureListControl : UserControl
    {
        private Assembly program;

        public ProcedureListControl()
        {
            InitializeComponent();
        }

        public Assembly Program
        {
            get { return program; }
            set
            {
                program = value;
                if (program == null)
                    this.DataContext = null;
                else
                    this.DataContext = new ProcedureListViewModel(program);
            }
        }

        #region Navigation

        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is ListViewItem))
                return;

            var item = ((ListViewItem)sender).Content as ProcedureListViewModel.ProcedureItem;
            if (item == null)
                return;

            Uri uri = item.Uri;
            string targetName = GetTargetNameFromModifierKeys();
            RaiseRequestNavigate(uri, targetName);
        }

        private static string GetTargetNameFromModifierKeys()
        {
            switch (Keyboard.Modifiers)
            {
                default:
                case ModifierKeys.None:
                    return "asm";
                case ModifierKeys.Control:
                    return "asm:_blank";
                case ModifierKeys.Shift:
                    return "hex";
                case ModifierKeys.Control | ModifierKeys.Shift:
                    return "hex:_blank";
            }
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperlink = sender as Hyperlink;
            if (hyperlink != null)
            {
                // Select the containing ListViewItem.
                ListViewItem item = lvProcedures.ItemContainerGenerator.ContainerFromItem(
                    hyperlink.DataContext) as ListViewItem;
                if (item != null)
                {
                    item.IsSelected = true;
                    item.Focus();
                }

                // Raise RequestNavigate event.
                Uri uri = hyperlink.NavigateUri;
                string targetName = GetTargetNameFromModifierKeys();
                RaiseRequestNavigate(uri, targetName);
            }
        }

#if false
        private static T FindParent<T>(DependencyObject element)
            where T : DependencyObject
        {
            while (element != null)
            {
                element = System.Windows.Media.VisualTreeHelper.GetParent(element);
                if (element is T)
                    return (T)element;
            }
            return null;
        }
#endif

        private void RaiseRequestNavigate(Uri uri, string targetName)
        {
            if (RequestNavigate != null && uri != null)
            {
                RequestNavigateEventArgs e = new RequestNavigateEventArgs(uri, targetName);
                RequestNavigate(this, e);
            }
        }

        private void ListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var item = lvProcedures.SelectedItem as ProcedureListViewModel.ProcedureItem;
                if (item == null)
                    return;

                e.Handled = true;
                Uri uri = item.Uri;
                string targetName = GetTargetNameFromModifierKeys();
                RaiseRequestNavigate(uri, targetName);
            }
        }

        private void OpenLinkCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenDisassemblyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RaiseRequestNavigate(e.Parameter as Uri, "asm");
        }

        private void OpenNewDisassemblyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RaiseRequestNavigate(e.Parameter as Uri, "asm:_blank");
        }

        private void OpenHexViewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RaiseRequestNavigate(e.Parameter as Uri, "hex");
        }

        private void OpenNewHexViewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            RaiseRequestNavigate(e.Parameter as Uri, "hex:_blank");
        }

        public event EventHandler<RequestNavigateEventArgs> RequestNavigate;

        #endregion
    }

    class ProcedureListViewModel
    {
        public List<ProcedureItem> Items { get; private set; }

        public ProcedureListViewModel(Assembly program)
        {
            this.Items = new List<ProcedureItem>();
            foreach (Procedure proc in program.Procedures)
            {
                ProcedureItem viewItem = new ProcedureItem(proc);
                Items.Add(viewItem);
            }
        }

        public class ProcedureItem
        {
            readonly Procedure procedure;

            public ProcedureItem(Procedure procedure)
            {
                if (procedure == null)
                    throw new ArgumentNullException("procedure");
                this.procedure = procedure;
            }

            public Procedure Procedure
            {
                get { return procedure; }
            }

            public string Name
            {
                get { return procedure.Name; }
            }

            public Address EntryPoint
            {
                get { return procedure.EntryPoint; }
            }

            public int Size
            {
                get { return -1; /* Procedure.Size; */ }
            }

            public Uri Uri
            {
                get
                {
                    return new Uri(string.Format("ddd://document1#{0}", EntryPoint));
                }
            }
        }
    }
}
