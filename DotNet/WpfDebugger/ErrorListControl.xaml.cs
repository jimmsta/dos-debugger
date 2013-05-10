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
    /// Interaction logic for ErrorListControl.xaml
    /// </summary>
    public partial class ErrorListControl : UserControl
    {
        public ErrorListControl()
        {
            InitializeComponent();
        }

        private BinaryImage image;
        private ErrorViewItem[] viewItems;

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
            this.viewItems = null;
            this.lvErrors.ItemsSource = null;
            if (image == null)
                return;

            this.viewItems = (from Error error in image.Errors
                              orderby error.Location.LinearAddress
                              select new ErrorViewItem(error)).ToArray();

            this.lvErrors.ItemsSource = viewItems;
            //DisplayErrors();
        }

#if false
        private void lvErrors_DoubleClick(object sender, EventArgs e)
        {
            if (lvErrors.SelectedIndices.Count == 1)
            {
                Error error = (Error)lvErrors.SelectedItems[0].Tag;
                document.Navigator.SetLocation(error.Location, this);
            }
        }
#endif

#if false
        private void DisplayErrors(ErrorCategory category)
        {
            lvErrors.Items.Clear();
            if (errors == null)
                return;

            int errorCount = 0, warningCount = 0, messageCount = 0;
            foreach (Error error in errors)
            {
                if ((error.Category & category) != 0)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = error.Location.ToString();
                    item.SubItems.Add(error.Message);
                    item.Tag = error;
                    lvErrors.Items.Add(item);
                }
                switch (error.Category)
                {
                    case ErrorCategory.Error: errorCount++; break;
                    case ErrorCategory.Warning: warningCount++; break;
                    case ErrorCategory.Message: messageCount++; break;
                }
            }

            btnErrors.Text = errorCount + " Errors";
            btnWarnings.Text = warningCount + " Warnings";
            btnMessages.Text = messageCount + " Messages";

            btnErrors.Enabled = (errorCount > 0);
            btnWarnings.Enabled = (warningCount > 0);
            btnMessages.Enabled = (messageCount > 0);
        }

        private void DisplayErrors()
        {
            ErrorCategory category = ErrorCategory.None;
            if (btnErrors.Checked)
                category |= ErrorCategory.Error;
            if (btnWarnings.Checked)
                category |= ErrorCategory.Warning;
            if (btnMessages.Checked)
                category |= ErrorCategory.Message;
            DisplayErrors(category);
        }

        private void btnErrorCategory_CheckedChanged(object sender, EventArgs e)
        {
            DisplayErrors();
        }
#endif
    }

    class ErrorViewItem
    {
        public Error Error { get; private set; }

        public ErrorViewItem(Error error)
        {
            this.Error = error;
        }

        public Pointer Location
        {
            get { return Error.Location; }
        }

        public string Message
        {
            get { return Error.Message; }
        }
    }
}
