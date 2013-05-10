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
            lvProcedures.ItemsSource = null;
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

            lvProcedures.ItemsSource = viewItems;
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
