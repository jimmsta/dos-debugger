using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            //if (Control.ModifierKeys == ModifierKeys.Control)
            if (Keyboard.Modifiers == ModifierKeys.Control)
                MessageBox.Show("Control+Click");
            else
                MessageBox.Show("Click");
        }
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
    }
}
