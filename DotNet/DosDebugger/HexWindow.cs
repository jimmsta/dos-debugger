using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Util;
using System.Runtime.InteropServices;

namespace DosDebugger
{
    public partial class HexWindow : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        public HexWindow()
        {
            InitializeComponent();
        }

        private Document document;
        //private ListingViewModel listingView;

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
            // This routine is a huge memory eater.
            return;
#if false
            richTextBox1.Clear();
            if (document == null)
                return;

            listingView = new ListingViewModel(document.Disassembler);

            StringBuilder sb = new StringBuilder();
            int rowCount = listingView.Rows.Count;
            int[] rowStart = new int[rowCount + 1];
            for (int i = 0; i < rowCount; i++)
            {
                ListingRow row = listingView.Rows[i];
                rowStart[i] = sb.Length;
                sb.AppendFormat("{0} {1}\n", row.Location, row.Text);
            }
            rowStart[rowCount] = sb.Length;
            richTextBox1.Text = sb.ToString();
            return;
#if false
#if false
            //richTextBox1.Test();
#else
            // Format the text.
            //richTextBox1.Visible = false;
            ITextDocument textDocument = richTextBox1.GetTextDocument();
            //textDocument.Freeze();
            int nUndoLimit = richTextBox1.SetUndoLimit(0);
            //textDocument.BeginEditCollection(); -- not implemented
#if false
            richTextBox1.SelectAll();
            richTextBox1.SelectionColor = Color.Blue;
            richTextBox1.DeselectAll();
#else
            //richTextBox1.SelectedRtf
            System.Diagnostics.Debug.WriteLine("Formatting " + rowCount + " lines...");
            for (int i = rowCount - 1; i >= 0; i--)
            {
                richTextBox1.Select(rowStart[i], 9);
                //richTextBox1.SelectionColor = Color.Blue;
                richTextBox1.SelectedText = "NEW";
            }
#endif
            //richTextBox1.Visible = true;
            //textDocument.Unfreeze();
            //textDocument.EndEditCollection();
#endif
#endif
#endif
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

    public static class RichTextBoxExtensions
    {
        public static ITextDocument GetTextDocument(this RichTextBox richTextBox)
        {
            if (richTextBox == null)
                throw new ArgumentNullException("richTextBox");

            IntPtr hWnd = richTextBox.Handle;
            ITextDocument textDocument;
            if (SendMessage(hWnd, EM_GETOLEINTERFACE, IntPtr.Zero, out textDocument) == IntPtr.Zero)
                throw new InvalidOperationException("Cannot retrieve the COM interface.");
            if (textDocument == null)
                throw new InvalidOperationException("The control returned a null pointer.");

            return textDocument;
        }

        public static int SetUndoLimit(this RichTextBox richTextBox, int limit)
        {
            IntPtr ret = SendMessage(
                richTextBox.Handle, 
                EM_SETUNDOLIMIT,
                new IntPtr(limit), 
                IntPtr.Zero);
            return ret.ToInt32();
        }

        public static void Test(this RichTextBox richTextBox)
        {
            IntPtr hWnd = richTextBox.Handle;
            
            ITextDocument textDocument = GetTextDocument(richTextBox);

            string name = textDocument.GetName();
            float tabStop = textDocument.GetDefaultTabStop();
            int n1 = textDocument.Freeze();
            int n2 = textDocument.Unfreeze();
            Marshal.ReleaseComObject(textDocument);
        }

        const int WM_USER = 0x0400;
        const int EM_GETOLEINTERFACE = WM_USER + 60;
        const int EM_SETUNDOLIMIT = WM_USER + 82;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, out IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, out ITextDocument lParam);
    }

    [Guid("8CC497C0-A1DF-11ce-8098-00AA0047BE5D")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface ITextDocument
    {
        string GetName();
        object GetSelection(); // ITextSelection
        int GetStoryCount();
        object GetStoryRanges(); // ITextStoryRanges
        int GetSaved();
        void SetSaved(int value);

        /// <summary>
        /// Gets the default tab stop, measured in points (1/72 inch).
        /// </summary>
        /// <returns>The default tab stop.</returns>
        float GetDefaultTabStop();

        /// <summary>
        /// Sets the default tab stop, measured in points (1/72 inch).
        /// </summary>
        /// <param name="value">The default tab stop.</param>
        void SetDefaultTabStop(float value);

        void New();
        void Open(object pVar, int flags, int codePage); // VARIANT *
        void Save(object pVar, int flags, int codePage);

        /// <summary>
        /// Increments the freeze count. When the freeze count is nonzero,
        /// screen updating is disabled.
        /// </summary>
        /// <returns>The updated freeze count.</returns>
        int Freeze();

        /// <summary>
        /// Decrements the freeze count. Note that if edit collection is 
        /// active, screen updating is suppressed even if the freeze count
        /// is zero.
        /// </summary>
        /// <returns>The updated freeze count.</returns>
        int Unfreeze();

        /// <summary>
        /// Turns on edit collection (also called undo grouping).
        /// </summary>
        void BeginEditCollection();

        /// <summary>
        /// Turns off edit collection (also called undo grouping).
        /// </summary>
        void EndEditCollection();
        
        int Undo(int count);
        int Redo(int count);
        //[return: MarshalAs(UnmanagedType.Interface)]
        object Range(int cpActive, int cpAnchor); // ITextRange
        //[return: MarshalAs(UnmanagedType.Interface)]
        object RangeFromPoint(int x, int y); // ITextRange
    }
}
