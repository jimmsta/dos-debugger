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
        private List<ProcedureListItem> viewItems;

        public BinaryImage Image
        {
            get { return image; }
            set
            {
                image = value;
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            gridProcedures.ItemsSource = null;
            if (image == null)
                return;

            this.viewItems = new List<ProcedureListItem>();
            foreach (Procedure proc in image.Procedures)
            {
                ProcedureListItem viewItem = new ProcedureListItem(proc);
                viewItems.Add(viewItem);
                //item.SubItems.Add(proc.ByteRange.Intervals.Count.ToString());
                //item.SubItems.Add(proc.ByteRange.Length.ToString());
            }

            gridProcedures.ItemsSource = viewItems;
        }

        #region Navigation

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ProcedureListItem item = gridProcedures.CurrentItem as ProcedureListItem;
            if (item == null)
                return;

            Uri uri = item.Uri;
            string targetName = GetTargetNameFromModifierKeys();
            RaiseRequestNavigate(uri, targetName);
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            Uri uri = (sender as System.Windows.Documents.Hyperlink).NavigateUri;
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

        private void mnuContextOpenLink_Click(object sender, RoutedEventArgs e)
        {
            ProcedureListItem item = gridProcedures.CurrentItem as ProcedureListItem;
            if (item == null)
                return;

            string targetName;
            switch ((sender as MenuItem).Name)
            {
                default:
                case "mnuContextOpenDisassembly":
                    targetName = "asm";
                    break;
                case "mnuContextOpenDisassemblyInNewTab":
                    targetName = "asm:_blank";
                    break;
                case "mnuContextOpenHexView":
                    targetName = "hex";
                    break;
                case "mnuContextOpenHexViewInNewTab":
                    targetName = "hex:_blank";
                    break;
            }
            RaiseRequestNavigate(item.Uri, targetName);
        }

        private void RaiseRequestNavigate(Uri uri, string targetName)
        {
            if (RequestNavigate != null)
            {
                RequestNavigateEventArgs e = new RequestNavigateEventArgs(uri, targetName);
                RequestNavigate(this, e);
            }
        }

        private void gridProcedures_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ProcedureListItem item = gridProcedures.CurrentItem as ProcedureListItem;
                if (item == null)
                    return;

                e.Handled = true;
                Uri uri = item.Uri;
                string targetName = GetTargetNameFromModifierKeys();
                RaiseRequestNavigate(uri, targetName);
            }
        }

        public event EventHandler<RequestNavigateEventArgs> RequestNavigate;

        #endregion
    }

    class ProcedureListItem
    {
        public ProcedureListItem(Procedure procedure)
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
