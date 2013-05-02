using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DosDebugger
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            Properties.Resources.ResourceManager.ReleaseAllResources();
        }
    }
}
