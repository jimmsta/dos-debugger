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
                this.SelectedObject = image;
            }
        }

#if false
        public object SelectedObject
        {
            get { return propertyGrid.SelectedObject; }
            set { propertyGrid.SelectedObject = value; }
        }
#endif
        public object SelectedObject { get; set; }

        private int[] testArray;
        public int[] TestArray
        {
            get
            {
                if (testArray == null)
                {
                    testArray = new int[100];
                    for (int i = 0; i < testArray.Length; i++)
                    {
                        testArray[i] = i * 2 + 1;
                    }
                }
                return testArray;
            }
        }
    }
}
