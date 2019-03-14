using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace CrmWebResourcesUpdater.Common
{
    public static class Logger
    {
        private static IVsOutputWindow _outputWindow;

        /// <summary>
        /// Initialize Logger output window
        /// </summary>
        public static void Initialize()
        {
            _outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
            var windowGuid = new Guid(ProjectGuids.OutputWindowGuidString);
            var windowTitle = "Crm Publisher";
            _outputWindow.CreatePane(ref windowGuid, windowTitle, 1, 1);
        }

        /// <summary>
        /// Adds line feed to message and writes it to output window
        /// </summary>
        /// <param name="message">text message to write</param>
        /// <param name="print">print or ignore call using for extended logging</param>
        public static void WriteLine(string message, bool print = true)
        {
            if(print)
            {
                Write(message + "\r\n");
            }
        }

        /// <summary>
        /// Writes message to output window
        /// </summary>
        /// <param name="message">Text message to write</param>
        public static void Write(string message)
        {
            var windowGuid = new Guid(ProjectGuids.OutputWindowGuidString);
            IVsOutputWindowPane pane;
            _outputWindow.GetPane(ref windowGuid, out pane);
            pane.Activate();
            pane.OutputString(message);
        }

        public static void Clear()
        {
            var windowGuid = new Guid(ProjectGuids.OutputWindowGuidString);
            IVsOutputWindowPane pane;
            _outputWindow.GetPane(ref windowGuid, out pane);
            pane.Clear();
        }

        public static void WriteLineWithTime(string message, bool print = true)
        {
            WriteLine(DateTime.Now.ToString("HH:mm") + ": " + message, print);
        }
    }
}
