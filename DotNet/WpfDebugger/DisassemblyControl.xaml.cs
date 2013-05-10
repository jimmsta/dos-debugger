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
            lvListing.ItemsSource = null;
            if (image == null)
                return;

            this.viewModel = new ListingViewModel(image);
            lvListing.ItemsSource = viewModel.Rows;
        }
    }
}
