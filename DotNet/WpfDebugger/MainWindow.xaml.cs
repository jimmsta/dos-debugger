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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        BinaryImage image;
        //ListingViewModel viewModel;

        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            //lvData.ItemsSource = viewModel.Rows;
            //procedureList.Image = image;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string fileName = @"E:\Dev\Projects\DosDebugger\Test\H.EXE";
            MZFile mzFile = new MZFile(fileName);
            mzFile.Relocate(0);
            Disassembler16 dasm = new Disassembler16(mzFile.Image, mzFile.BaseAddress);
            dasm.Analyze(mzFile.EntryPoint);

            this.image = dasm.Image;
            //this.viewModel = new ListingViewModel(dasm.Image);
        }

        private void mnuHelpTest_Click(object sender, RoutedEventArgs e)
        {
            this.disassemblyList.Image = image;
            this.procedureList.Image = image;
        }
    }
}
