using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DosDebugger
{
    public partial class HexWindow : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public HexWindow()
        {
            InitializeComponent();
        }

        private Document document;
        private ListingViewModel listingView;

        internal Document Document
        {
            get { return this.document; }
            set
            {
                this.document = value;
                UpdateUI();
            }
        }

        public void UpdateUI()
        {
            richTextBox1.Clear();
            if (document == null)
                return;

            listingView = new ListingViewModel(document.Disassembler);

            StringBuilder sb = new StringBuilder();
            foreach (ListingRow row in listingView.Rows)
            {
                sb.AppendLine(row.Text);
            }
            richTextBox1.Text = sb.ToString();
        }

        private void HexWindow_Load(object sender, EventArgs e)
        {
#if false
            // Repeat the text of the rich edit until 10MB in size.
            string s = richTextBox1.Text;
            StringBuilder sb = new StringBuilder(100000);
            while (sb.Length < sb.Capacity - s.Length)
            {
                sb.Append(s);
            }
            System.Diagnostics.Debug.WriteLine(sb.Length + " bytes");
            richTextBox1.Text = sb.ToString();

            richTextBox1.LoadFile("Sample.RTF");
#endif
        }

        private void richTextBox1_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            MessageBox.Show("Link Text: " + e.LinkText);
        }
    }
}
