using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
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
        public ProcedureListControl()
        {
            InitializeComponent();
        }

        private BinaryImage image;

        public BinaryImage Image
        {
            get { return image; }
            set
            {
                image = value;
                if (image == null)
                    this.DataContext = null;
                else
                    this.DataContext = new ProcedureListViewModel(image);
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
            Uri uri = (sender as System.Windows.Documents.Hyperlink).NavigateUri;
            string targetName = GetTargetNameFromModifierKeys();
            RaiseRequestNavigate(uri, targetName);
        }

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

        public ProcedureListViewModel(BinaryImage image)
        {
            this.Items = new List<ProcedureItem>();
            foreach (Procedure proc in image.Procedures)
            {
                ProcedureItem viewItem = new ProcedureItem(proc);
                Items.Add(viewItem);
            }
        }

        public class ProcedureItem
        {
            public ProcedureItem(Procedure procedure)
            {
                if (procedure == null)
                    throw new ArgumentNullException("procedure");
                this.Procedure = procedure;
            }

            public Procedure Procedure { get; private set; }

            public string Name
            {
                get { return string.Format("sub_{0}", Procedure.EntryPoint.LinearAddress); }
            }

            public Pointer EntryPoint
            {
                get { return Procedure.EntryPoint; }
            }

            public int Size
            {
                get { return Procedure.Size; }
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
