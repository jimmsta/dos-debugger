using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Disassembler;

namespace WpfDebugger
{
    /// <summary>
    /// Interaction logic for DisassemblyControl.xaml
    /// </summary>
    public partial class DisassemblyControl : UserControl
    {
        public DisassemblyControl()
        {
            InitializeComponent();
        }

        private BinaryImage image;
        private ListingViewModel viewModel;

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
            this.DataContext = null;
            if (image == null)
                return;

            this.viewModel = new ListingViewModel(image);
            //gridListing.ItemsSource = viewModel.Rows;
            this.DataContext = viewModel;
        }

        public void GoToAddress(Pointer address)
        {
            int index = viewModel.FindRowIndex(address.LinearAddress);

            // Scroll to the bottom first so that the actual item will be
            // on the top when we scroll again.
            lvListing.ScrollIntoView(viewModel.Rows[viewModel.Rows.Count - 1]);

            // We must UpdateLayout() now, otherwise the first dummy scroll
            // won't have any effect.
            lvListing.UpdateLayout();

            // Now scroll the actual item into view.
            lvListing.ScrollIntoView(viewModel.Rows[index]);

            // Select the item.
            lvListing.SelectedIndex = index;

            // Note: we MUST get the ListViewItem and call Focus() on this
            // item. If we instead call Focus() on lvListing, the UI will
            // hang if
            //   1) the focused item is out of the screen, and
            //   2) we press Up/Down arrow.
            // The reason is probably that there is no ListViewItem created
            // for an off-the-screen row, and somehow WPF chokes on this.
            ListViewItem item = lvListing.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
            if (item != null)
            {
                item.Focus();
            }
        }

        private void ChildHyperlink_Click(object sender, RoutedEventArgs e)
        {
            Hyperlink hyperlink = e.OriginalSource as Hyperlink;
            if (hyperlink != null)
            {
                MessageBox.Show(string.Format(
                    "Hyperlink clicked: Uri={0}", hyperlink.NavigateUri));
            }
        }
    }
}
