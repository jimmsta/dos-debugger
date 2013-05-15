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
    /// Interaction logic for PropertiesControl.xaml
    /// </summary>
    public partial class PropertiesControl : UserControl
    {
        public PropertiesControl()
        {
            InitializeComponent();
        }

        BinaryImage image;

        public BinaryImage Image
        {
            get { return image; }
            set
            {
                image = value;
                // The PropertyGrid control is extremely slow when
                // a property has type byte[]. Need to fix this before
                // we use it at all.
                //UpdateUI();
            }
        }

        public object SelectedObject
        {
            get { return propertyGrid1.SelectedObject; }
            set
            {
                propertyGrid1.SelectedObject = value;
                propertyGrid.SelectedObject = value;
            }
        }

        private void UpdateUI()
        {
            if (image == null)
                return;

            propertyGrid1.SelectedObject = image;
        }
    }
}
