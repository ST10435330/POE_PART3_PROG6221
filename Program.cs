using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Threading;
using CybersecurityBotGUI;
using System.Windows.Forms;

namespace CybersecurityAwarenessBot
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

            // Launch Form1 as the main window
            Application.Run(new Form1());
        }
    }
}

