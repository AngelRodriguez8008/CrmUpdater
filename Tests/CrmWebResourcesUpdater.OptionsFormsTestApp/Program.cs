using System;
using System.Windows.Forms;
using CrmWebResourcesUpdater.OptionsForms;

namespace CrmWebResourcesUpdater.OptionsFormsTestApp
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new WebResourcesUpdaterForm());
            }
            catch (Exception ex)
            {
                string message = ex.ToString();
                string caption = "Error";
                MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}