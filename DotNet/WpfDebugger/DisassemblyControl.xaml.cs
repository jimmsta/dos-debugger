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
            //var row = viewModel.Rows[index];
            gridListing.SelectedIndex = index;

            // This will make sure the item is scrolled to the top of
            // the visible rows.
            gridListing.ScrollIntoView(viewModel.Rows[viewModel.Rows.Count - 1]);
            gridListing.UpdateLayout();
            gridListing.ScrollIntoView(viewModel.Rows[index]);
            gridListing.Focus();
        }
    }
}
