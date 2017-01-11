using System;
using System.Windows.Forms;

namespace Sketchball
{
    static class Program
    {
        public static bool ReleaseMode = true;
        public static bool IsFourPlayerMode = false;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SelectionForm());
        }
    }
}