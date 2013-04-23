using Disassembler;
using System;
using System.Windows.Forms;

namespace DosDebugger
{
    public partial class ErrorWindow : ToolWindow
    {
        public ErrorWindow()
        {
            InitializeComponent();
        }

        private Document document;
        
        internal Document Document
        {
            get { return this.document; }
            set
            {
                this.document = value;
                UpdateUI();
            }
        }

        private void UpdateUI()
        {
            lvErrors.Items.Clear();
            if (document == null)
                return;

            Disassembler16 dasm = document.Disassembler;
            Error[] errors = dasm.Errors;
            Array.Sort(errors, new ErrorLocationComparer());
            foreach (Error error in errors)
            {
                ListViewItem item = new ListViewItem();
                item.Text = error.Location.ToString();
                item.SubItems.Add(error.Message);
                item.Tag = error;
                lvErrors.Items.Add(item);
            }
        }

        private void lvErrors_DoubleClick(object sender, EventArgs e)
        {
            if (lvErrors.SelectedIndices.Count == 1)
            {
                if (NavigationRequested != null)
                {
                    Error error = (Error)lvErrors.SelectedItems[0].Tag;
                    NavigationRequestedEventArgs args = new NavigationRequestedEventArgs(error.Location);
                    NavigationRequested(this, args);
                }
            }
        }

        public EventHandler<NavigationRequestedEventArgs> NavigationRequested { get; set; }
    }
}
