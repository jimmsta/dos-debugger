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
        private ErrorListViewModel viewModel;

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
            this.viewModel = new ErrorListViewModel(image);

            //this.lvErrors.ItemsSource = viewItems;
            //this.txtError.DataContext = this;
            this.DataContext = viewModel;
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

    class ErrorListViewModel
    {
        public ErrorViewItem[] Items { get; private set; }
        public int ErrorCount { get; private set; }
        public int WarningCount { get; private set; }
        public int MessageCount { get; private set; }

        public ErrorListViewModel(BinaryImage image)
        {
            if (image == null)
                return;

            int errorCount = 0;
            int warningCount = 0;
            int messageCount = 0;

            List<ErrorViewItem> items = new List<ErrorViewItem>();
            foreach (Error error in image.Errors)
            {
                items.Add(new ErrorViewItem(error));
#if false
                if ((error.Category & category) != 0)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = error.Location.ToString();
                    item.SubItems.Add(error.Message);
                    item.Tag = error;
                    lvErrors.Items.Add(item);
                }
#endif
                switch (error.Category)
                {
                    case ErrorCategory.Error: errorCount++; break;
                    case ErrorCategory.Warning: warningCount++; break;
                    case ErrorCategory.Message: messageCount++; break;
                }
            }
            items.Sort((x, y) => x.Error.Location.LinearAddress.CompareTo(y.Error.Location.LinearAddress));

            this.Items = items.ToArray();
            this.ErrorCount = errorCount;
            this.WarningCount = warningCount;
            this.MessageCount = messageCount;
        }
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
