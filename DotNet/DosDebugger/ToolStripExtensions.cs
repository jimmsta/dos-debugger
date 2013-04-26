using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace DosDebugger
{
    public static class ToolStripExtensions
    {
        public static void ClearAndDispose(this ToolStripItemCollection items)
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                ToolStripItem item = items[i];
                items.RemoveAt(i);
                item.Dispose();
            }
        }
    }
}
