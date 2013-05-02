using Disassembler;
using System;
using System.Windows.Forms;
using System.Collections.Generic;

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

        Error[] errors;

        private void UpdateUI()
        {
            lvErrors.Items.Clear();
            errors = null;
            if (document == null)
                return;

            Disassembler16 dasm = document.Disassembler;
            errors = dasm.Errors;
            Array.Sort(errors, Error.CompareByLocation);
            DisplayErrors();
#if false
            foreach (Error error in errors)
            {
                ListViewItem item = new ListViewItem();
                item.Text = error.Location.ToString();
                item.SubItems.Add(error.Message);
                item.Tag = error;
                lvErrors.Items.Add(item);
            }
#endif
        }

        private void lvErrors_DoubleClick(object sender, EventArgs e)
        {
            if (lvErrors.SelectedIndices.Count == 1)
            {
                Error error = (Error)lvErrors.SelectedItems[0].Tag;
                document.Navigator.SetLocation(error.Location, this);
            }
        }

        //public EventHandler<NavigationRequestedEventArgs> NavigationRequested { get; set; }

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
    }
}
