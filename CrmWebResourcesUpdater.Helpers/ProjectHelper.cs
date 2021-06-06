using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace CrmWebResourcesUpdater.Helpers
{
    public static class ProjectHelper
    {
        public const string FileKindGuid =         "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";
        public const string ProjectKindGuid =      "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";

        private static IServiceProvider _serviceProvider;

        public static string GetProjectRoot(Project project)
        {
            return Path.GetDirectoryName(project.FullName)?.ToLower();
        }

        /// <summary>
        /// Sets service provider for helper needs
        /// </summary>
        /// <param name="serviceProvider">Service provider</param>
        public static void SetServiceProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Creates context menu command for extension
        /// </summary>
        /// <param name="comandSet">Guid for command set in context menu</param>
        /// <param name="commandID">Guid for command</param>
        /// <param name="invokeHandler">Handler for menu command</param>
        /// <returns>Returns context menu command</returns>
        public static OleMenuCommand GetMenuCommand(Guid comandSet, int commandID, EventHandler invokeHandler)
        {
            CommandID menuCommandID = new CommandID(comandSet, commandID);
            return new OleMenuCommand(invokeHandler, menuCommandID);
        }

        /// <summary>
        /// Gets Publisher settings for selected project
        /// </summary>
        /// <returns>Returns settings for selected project</returns>
        public static T GetSettings<T>()
        {
            var project = GetSelectedProject();
            var guid = GetProjectGuid(project);
            
            var cache = SettingsManager<T>.Cache;
            var success = cache.TryGetValue(guid, out T settings);
            if (success)
                return settings;

            var manager = new SettingsManager<T>(_serviceProvider, guid);
            settings = manager.GetSettings();
            if (settings != null)
                cache.Add(guid, settings);

            return settings;
        }
        
        /// <summary>
        /// Gets Publisher settings for selected project
        /// </summary>
        /// <returns>Returns settings for selected project</returns>
        public static void SaveSettings<T>(T settings)
        {
            var project = GetSelectedProject();
            var guid = GetProjectGuid(project);
            
            var cache = SettingsManager<T>.Cache;
            cache[guid] = settings;

            var manager = new SettingsManager<T>(_serviceProvider, guid);
            manager.Save(settings);
        }

        /// <summary>
        /// Shows Configuration Error Dialog
        /// </summary>
        /// <returns>Returns result of an error dialog</returns>
        public static DialogResult ShowErrorDialog()
        {
            var title = "Configuration error";
            var text = "It seems that Publisher has not been configured yet or connection is not selected.\r\n\r\n" +
            "We can open configuration window for you now or you can do it later by clicking \"Publish options\" in the context menu of the project.\r\n\r\n" +
            "Do you want to open configuration window now?";
            return MessageBox.Show(text, title, MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
        }

        /// <summary>
        /// Gets Guid of project
        /// </summary>
        /// <param name="project">Project to get guid of</param>
        /// <returns>Returns project guid</returns>
        private static Guid GetProjectGuid(Project project)
        {
            var solution = _serviceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            if(solution == null)
                return Guid.Empty;
            
            solution.GetProjectOfUniqueName(project.FullName, out var hierarchy);
            if (hierarchy == null)
                return Guid.Empty;

            solution.GetGuidOfProject(hierarchy, out var projectGuid);
            return projectGuid;
        }
        

        /// <summary>
        /// Gets selected project
        /// </summary>
        /// <returns>Returns selected project</returns>
        public static Project GetSelectedProject()
        {
            var dte = _serviceProvider.GetService(typeof(DTE)) as EnvDTE80.DTE2;
            if (dte == null)
            {
                throw new Exception("Failed to get DTE service.");
            }
            UIHierarchyItem uiHierarchyItem = ((object[])dte.ToolWindows.SolutionExplorer.SelectedItems).OfType<UIHierarchyItem>().FirstOrDefault();
            var project = uiHierarchyItem?.Object as Project;
            if (project == null)
            {
                var item = uiHierarchyItem?.Object as ProjectItem;
                project = item?.ContainingProject;
            }
            return project;
        }
        
        public static void SetStatusBar(string message, object icon = null)
        {
            var statusBar = _serviceProvider.GetService(typeof(SVsStatusbar)) as IVsStatusbar;
            if(statusBar == null)
                return;

            statusBar.IsFrozen(out int frozen);
            if (frozen == 0)
            {
                //object icon = (short)Microsoft.VisualStudio.Shell.Interop.Constants.SBAI_Deploy;
                if (icon != null)
                {
                    statusBar.Animation(1, ref icon);
                }
                //
                statusBar.SetText(message);
            }
        }

        public static void SaveAll()
        {
            var dte = _serviceProvider.GetService(typeof(DTE)) as EnvDTE80.DTE2;
            if (dte == null)
            {
                throw new Exception("Failed to get DTE service.");
            }
            dte.ExecuteCommand("File.SaveAll");
        }

        /// <summary>
        /// Reads encoded content from file
        /// </summary>
        /// <param name="filePath">Path to file to read content from</param>
        /// <returns>Returns encoded file contents as a System.String</returns>
        public static string GetEncodedFileContent(string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] binaryData = new byte[fs.Length];
            long bytesRead = fs.Read(binaryData, 0, (int)fs.Length);
            fs.Close();
            return Convert.ToBase64String(binaryData, 0, binaryData.Length);
        }

        /// <summary>
        /// Gets items selected in solution explorer
        /// </summary>
        /// <returns>Returns list of items which was selected in solution explorer</returns>
        public static List<ProjectItem> GetSelectedItems()
        {
            var dte = _serviceProvider.GetService(typeof(DTE)) as EnvDTE80.DTE2;
            if (dte == null)
            {
                throw new Exception("Failed to get DTE service.");
            }

            var selectedItems = (object[])dte.ToolWindows.SolutionExplorer.SelectedItems;
            var uiHierarchyItems = selectedItems.OfType<UIHierarchyItem>();
            var items = new List<ProjectItem>();
            foreach (var uiItem in uiHierarchyItems)
            {
                items.Add(uiItem.Object as ProjectItem);
            }
            return items;
        }

        /// <summary>
        /// Iterates through ProjectItems list and adds files paths to the output list
        /// </summary>
        /// <param name="list">List of project items</param>
        public static List<string> GetProjectFiles(List<ProjectItem> list)
        {
            if(list == null)
            {
                return null;
            }

            var files = new List<string>();
            foreach (ProjectItem item in list)
            {
                if (item.Kind.ToLower() == FileKindGuid.ToLower())
                {
                    var itemFileName = item.FileNames[0];
                    var path = Path.GetDirectoryName(itemFileName)?.ToLower();
                    var fileName = Path.GetFileName(itemFileName);
                    files.Add(path + "\\" + fileName);
                }

                if (item.ProjectItems != null)
                {
                    var childItems = GetProjectFiles(item.ProjectItems);
                    files.AddRange(childItems);
                }
            }

            return files;
        }

        /// <summary>
        /// Iterates through ProjectItems tree and adds files paths to the list
        /// </summary>
        /// <param name="projectItems">List of project items</param>
        public static List<string> GetProjectFiles(ProjectItems projectItems)
        {
            var list = new List<ProjectItem>();
            foreach (ProjectItem item in projectItems)
            {
                list.Add(item);
            }
            return GetProjectFiles(list);
        }

        public static List<string> GetSelectedFiles()
        {
            var selectedItems = GetSelectedItems();
            return GetProjectFiles(selectedItems);
        }

        public static string GetSelectedFilePath()
        {
            return GetSelectedFiles().FirstOrDefault();
        }

        public static List<string> GetProjectFiles()
        {
            var selectedProject = GetSelectedProject();
            if(selectedProject == null)
            {
                return null;
            }
            return GetProjectFiles(selectedProject.ProjectItems);
        }
    }
}
