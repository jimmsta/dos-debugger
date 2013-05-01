using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace DosDebugger
{
    public static class ToolStripExtensions
    {
        public static void ClearAndDispose(this ToolStripItemCollection items,
            int startIndex, int endIndex)
        {
            if (items == null)
                throw new ArgumentNullException("items");
            if (startIndex < 0 || startIndex > items.Count)
                throw new ArgumentOutOfRangeException("startIndex");
            if (endIndex < startIndex || endIndex > items.Count)
                throw new ArgumentOutOfRangeException("endIndex");

            for (int i = endIndex - 1; i >= startIndex; i--)
            {
                ToolStripItem item = items[i];
                items.RemoveAt(i);
                item.Dispose();
            }
        }

        public static void ClearAndDispose(this ToolStripItemCollection items)
        {
            ClearAndDispose(items, 0, items.Count);
        }
    }
}
