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
    /// Interaction logic for SegmentListControl.xaml
    /// </summary>
    public partial class SegmentListControl : UserControl
    {
        public SegmentListControl()
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
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            lvSegments.ItemsSource = null;
            if (image == null)
                return;

            var items = (from segment in image.Segments
                         select new SegmentViewItem(segment)
                         ).ToArray();
            lvSegments.ItemsSource = items;
        }

#if false
        private void lvSegments_DoubleClick(object sender, EventArgs e)
        {
            if (lvSegments.SelectedIndices.Count == 0)
                return;

            Segment segment = (Segment)lvSegments.SelectedItems[0].Tag;
            document.Navigator.SetLocation(
                segment.StartAddress.ToFarPointer(segment.SegmentAddress), this);
        }
#endif
    }

    class SegmentViewItem
    {
        public Segment Segment { get; private set; }

        public SegmentViewItem(Segment segment)
        {
            if (segment == null)
                throw new ArgumentNullException("segment");
            this.Segment = segment;
        }

        public string Start
        {
            get { return FormatAddress(Segment.StartAddress, Segment.SegmentAddress); }
        }

        public string End
        {
            get { return FormatAddress(Segment.EndAddress - 1, Segment.SegmentAddress); }
        }

        private static string FormatAddress(LinearPointer address, UInt16 segment)
        {
            return string.Format(
                "{0} ({1:X5})", new Pointer(segment, address), address);
        }
    }
}
