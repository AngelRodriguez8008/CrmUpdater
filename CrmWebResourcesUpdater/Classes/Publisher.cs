using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using McTools.Xrm.Connection;
using EnvDTE;
using Microsoft.Xrm.Sdk.Query;
using System.Threading.Tasks;
using CrmWebResourcesUpdater.Forms;
using System.Windows.Forms;
using System.ComponentModel;
using CrmWebResourcesUpdater.Common;
using CrmWebResourcesUpdater.Helpers;
using McTools.Xrm.Connection.WinForms;
using Microsoft.Crm.Sdk.Messages;

namespace CrmWebResourcesUpdater
{
    /// <summary>
    /// Provides methods for uploading and publishing web resources
    /// </summary>
    public class Publisher : IDisposable
    {
        protected const string FetchWebResourcesQueryTemplate = @"<fetch mapping='logical' count='500' version='1.0'>
                        <entity name='webresource'>
                            <attribute name='name' />
                            <attribute name='content' />
                            <link-entity name='solutioncomponent' from='objectid' to='webresourceid'>
                                <filter>
                                    <condition attribute='solutionid' operator='eq' value='{0}' />
                                </filter>
                            </link-entity>
                        </entity>
                    </fetch>";


        private readonly ConnectionDetail _connectionDetail;
        private readonly bool _autoPublish;
        private readonly bool _ignoreExtensions;
        private readonly bool _uploadSelectedItems;
        private readonly bool _extendedLog;

        private readonly IOrganizationService _orgService;

        /// <summary>
        /// Publisher constructor
        /// </summary>
        /// <param name="connection">Connection to CRM that will be used to upload webresources</param>
        /// <param name="uploadSelectedItems">Items to upload</param>
        /// <param name="autoPublish">Perform publishing or not</param>
        /// <param name="ignoreExtensions">Try to upload without extension if item not found with it</param>
        /// <param name="extendedLog">Print extended uploading process information</param>
        public Publisher(ConnectionDetail connection, bool uploadSelectedItems, bool autoPublish, bool ignoreExtensions, bool extendedLog = false) : this(connection, autoPublish, ignoreExtensions, extendedLog)
        {
            _uploadSelectedItems = uploadSelectedItems;
        }
        /*
        /// <summary>
        /// Publisher constructor
        /// </summary>
        /// <param name="connection">Connection to CRM that will be used to upload webresources</param>
        /// <param name="project">Project to upload files from</param>
        /// <param name="autoPublish">Perform publishing or not</param>
        /// <param name="ignoreExtensions">Try to upload without extension if item not found with it</param>
        /// <param name="extendedLog">Print extended uploading process information</param>
        public Publisher(ConnectionDetail connection, Project project, bool autoPublish, bool ignoreExtensions, bool extendedLog = false): this(connection, autoPublish, ignoreExtensions, extendedLog)
        {
            if(project == null)
            {
                throw new ArgumentNullException("project");
            }
            _project = project;
            _projectRootPath = Path.GetDirectoryName(_project.FullName);
        }
        */
        /// <summary>
        /// Publisher constructor
        /// </summary>
        /// <param name="connection">Connection to CRM that will be used to upload webresources</param>
        /// <param name="autoPublish">Perform publishing or not</param>
        /// <param name="ignoreExtensions">Try to upload without extension if item not found with it</param>
        /// <param name="extendedLog">Print extended uploading process information</param>
        private Publisher(ConnectionDetail connection, bool autoPublish, bool ignoreExtensions, bool extendedLog)
        {
            _connectionDetail = connection ?? throw new ArgumentNullException(nameof(connection));
            _autoPublish = autoPublish;
            _ignoreExtensions = ignoreExtensions;
            _extendedLog = extendedLog;
            _orgService = CrmConnectionHelper.GetOrganizationServiceProxy(_connectionDetail);
        }

        /// <summary>
        /// Reads files from project and starts Publish process asynchronously using selected connection
        /// </summary>
        public void PublishWebResourcesAsync()
        {
            Task.Run(() => PublishWebResources());
        }

        /// <summary>
        /// Uploads and publishes files to CRM
        /// </summary>
        public void PublishWebResources()
        {
            ProjectHelper.SaveAll();
            Logger.Clear();
            ProjectHelper.SetStatusBar("Uploading...");
            Logger.WriteLineWithTime(_autoPublish ? "Publishing web resources..." : "Uploading web resources...");

            Logger.WriteLine("Connecting to CRM...");
            Logger.WriteLine("URL: " + _connectionDetail.WebApplicationUrl);
            Logger.WriteLine("Solution Name: " + _connectionDetail.SolutionFriendlyName);
            Logger.WriteLine("--------------------------------------------------------------");

            Logger.WriteLine("Loading files paths", _extendedLog);
            var projectFiles = GetSelectedFiles();


            if (projectFiles == null || projectFiles.Count == 0)
            {
                Logger.WriteLine("Failed to load files paths", _extendedLog);
                return;
            }

            Logger.WriteLine(projectFiles.Count + " path" + (projectFiles.Count == 1 ? " was" : "s were") + " loaded", _extendedLog);

            try
            {
                Logger.WriteLine("Starting uploading process", _extendedLog);
                var webresources = UploadWebResources(projectFiles);
                Logger.WriteLine("Uploading process was finished", _extendedLog);

                if (webresources.Count > 0)
                {
                    Logger.WriteLine("--------------------------------------------------------------");
                    foreach (var name in webresources.Values)
                    {
                        Logger.WriteLine(name + " successfully uploaded");
                    }

                    Logger.WriteLine("Updating Solution Version ...");
                    var solutionVersion = VersionsHelper.UpdateSolution(_orgService, _connectionDetail.SolutionId);
                    Logger.WriteLine($"New Solution Version: {solutionVersion} ");
                }
                Logger.WriteLine("--------------------------------------------------------------");
                Logger.WriteLineWithTime(webresources.Count + " file" + (webresources.Count == 1 ? " was" : "s were") + " uploaded");


                if (_autoPublish)
                {
                    ProjectHelper.SetStatusBar("Publishing...");
                    PublishWebResources(webresources.Keys);
                }

                if (_autoPublish)
                {
                    ProjectHelper.SetStatusBar(webresources.Count + " web resource" + (webresources.Count == 1 ? " was" : "s were") + " published");
                }
                else
                {
                    ProjectHelper.SetStatusBar(webresources.Count + " web resource" + (webresources.Count == 1 ? " was" : "s were") + " uploaded");
                }

            }
            catch (Exception ex)
            {
                ProjectHelper.SetStatusBar("Failed to publish script" + (projectFiles.Count == 1 ? "" : "s"));
                Logger.WriteLine("Failed to publish script" + (projectFiles.Count == 1 ? "." : "s."));
                Logger.WriteLine(ex.Message);
                Logger.WriteLine(ex.StackTrace, _extendedLog);
            }
            Logger.WriteLineWithTime("Done.");
        }



        /// <summary>
        /// Dispose object
        /// </summary>
        public void Dispose()
        {

        }


        public List<string> GetSelectedFiles()
        {
            List<string> projectFiles;
            if (_uploadSelectedItems)
            {
                Logger.WriteLine("Loading selected files' paths", _extendedLog);
                projectFiles = ProjectHelper.GetSelectedFiles();
            }
            else
            {
                Logger.WriteLine("Loading all files' paths", _extendedLog);
                projectFiles = ProjectHelper.GetProjectFiles();
            }

            return projectFiles;
        }

        /// <summary>
        /// Uploads web resources
        /// </summary>
        /// <returns>List of guids of web resources that was updated</returns>
        private Dictionary<Guid, string> UploadWebResources()
        {
            var projectFiles = GetSelectedFiles();

            if (projectFiles == null || projectFiles.Count == 0)
            {
                return null;
            }

            return UploadWebResources(projectFiles);
        }

        /// <summary>
        /// Uploads web resources
        /// </summary>
        /// <param name="projectFiles"></param>
        /// <returns>List of guids of web resources that was updateds</returns>            
        private Dictionary<Guid, string> UploadWebResources(List<string> projectFiles)
        {
            var ids = new Dictionary<Guid, string>();

            var project = ProjectHelper.GetSelectedProject();
            var projectRootPath = ProjectHelper.GetProjectRoot(project);
            var mappings = MappingHelper.LoadMappings(project);
            var webResources = RetrieveWebResources();

            foreach (var filePath in projectFiles)
            {
                var webResourceName = Path.GetFileName(filePath);
                var lowerFilePath = filePath.ToLower();

                if (string.Equals(webResourceName, Settings.MappingFileName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (mappings != null && mappings.ContainsKey(lowerFilePath))
                {
                    webResourceName = mappings[lowerFilePath];
                    var relativePath = lowerFilePath.Replace(projectRootPath + "\\", "");
                    Logger.WriteLine("Mapping found: " + relativePath + " to " + webResourceName, _extendedLog);
                }

                var webResource = webResources.FirstOrDefault(x => x.GetAttributeValue<string>("name") == webResourceName);
                if (webResource == null && _ignoreExtensions)
                {
                    Logger.WriteLine(webResourceName + " does not exists in selected solution", _extendedLog);
                    webResourceName = Path.GetFileNameWithoutExtension(filePath);
                    Logger.WriteLine("Searching for " + webResourceName, _extendedLog);
                    webResource = webResources.FirstOrDefault(x => x.GetAttributeValue<string>("name") == webResourceName);
                }
                if (webResource == null)
                {
                    Logger.WriteLine("Uploading of " + webResourceName + " was skipped: web resource does not exists in selected solution", _extendedLog);
                    Logger.WriteLine(webResourceName + " does not exists in selected solution", !_extendedLog);
                    continue;
                }
                if (!File.Exists(lowerFilePath))
                {
                    Logger.WriteLine("Warning: File not found: " + lowerFilePath);
                    continue;
                }
                var isUpdated = UpdateWebResourceByFile(webResource, filePath);
                if (isUpdated)
                {
                    ids.Add(webResource.Id, webResourceName);
                }
            }
            return ids;
        }



        /// <summary>
        /// Uploads web resource
        /// </summary>
        /// <param name="webResource">Web resource to be updated</param>
        /// <param name="filePath">File with a content to be set for web resource</param>
        /// <returns>Returns true if web resource is updated</returns>
        private bool UpdateWebResourceByFile(Entity webResource, string filePath)
        {
            var webResourceName = Path.GetFileName(filePath);
            Logger.WriteLine("Uploading " + webResourceName, _extendedLog);

            var project = ProjectHelper.GetSelectedProject();
            var projectRootPath = ProjectHelper.GetProjectRoot(project);

            var localContent = ProjectHelper.GetEncodedFileContent(filePath);
            var remoteContent = webResource.GetAttributeValue<string>("content");

            var hasContentChanged = remoteContent.Length != localContent.Length || remoteContent != localContent;
            if (hasContentChanged == false)
            {
                Logger.WriteLine("Uploading of " + webResourceName + " was skipped: there aren't any change in the web resource", _extendedLog);
                Logger.WriteLine(webResourceName + " has no changes", !_extendedLog);
                return false;
            }

            var version = VersionsHelper.UpdateJS(filePath);
            if (version != null)
            {
                version = $", new version => {version}";
                localContent = ProjectHelper.GetEncodedFileContent(filePath); // reload with new version
            }
            else
                version = string.Empty;
            
            UpdateWebResourceByContent(webResource, localContent);
            var relativePath = filePath.Replace(projectRootPath + "\\", "");
            webResourceName = webResource.GetAttributeValue<string>("name");
            Logger.WriteLine($"{webResourceName} uploaded from {relativePath}{version}", !_extendedLog);
            return true;
        }

        /// <summary>
        /// Uploads web resource
        /// </summary>
        /// <param name="webResource">Web resource to be updated</param>
        /// <param name="content">Content to be set for web resource</param>
        private void UpdateWebResourceByContent(Entity webResource, string content)
        {
            var name = webResource.GetAttributeValue<string>("name");
            webResource["content"] = content;
            _orgService.Update(webResource);

            Logger.WriteLine(name + " was successfully uploaded", _extendedLog);
        }

        /// <summary>
        /// Retrieves web resources for selected items
        /// </summary>
        /// <returns>List of web resources</returns>
        private List<Entity> RetrieveWebResources()
        {
            Logger.WriteLine("Retrieving existing web resources", _extendedLog);
            var fetchQuery = string.Format(FetchWebResourcesQueryTemplate, _connectionDetail.SolutionId);
            var response = _orgService.RetrieveMultiple(new FetchExpression(fetchQuery));
            var webResources = response.Entities.ToList();

            return webResources;
        }

        /// <summary>
        /// Publish webresources changes
        /// </summary>
        /// <param name="webresourcesIds">List of webresource IDs to publish</param>
        private void PublishWebResources(IEnumerable<Guid> webresourcesIds)
        {
            Logger.WriteLineWithTime("Publishing...");
            if (webresourcesIds == null)
            {
                throw new ArgumentNullException(nameof(webresourcesIds));
            }

            var webresourcesIdsArr = webresourcesIds as Guid[] ?? webresourcesIds.ToArray();
            if (webresourcesIdsArr.Any())
            {
                var request = GetPublishRequest(webresourcesIdsArr);
                _orgService.Execute(request);
            }
            var count = webresourcesIdsArr.Length;
            Logger.WriteLineWithTime(count + " file" + (count == 1 ? " was" : "s were") + " published");
        }

        /// <summary>
        /// Returns publish request
        /// </summary>
        /// <param name="webresourcesIds">List of web resources IDs</param>
        /// <returns></returns>
        private OrganizationRequest GetPublishRequest(IEnumerable<Guid> webresourcesIds)
        {
            if (webresourcesIds == null)
                throw new ArgumentNullException(nameof(webresourcesIds));

            Guid[] webresourcesIdsArr = webresourcesIds as Guid[] ?? webresourcesIds.ToArray();
            if (webresourcesIdsArr.Length == 0)
                throw new ArgumentNullException(nameof(webresourcesIds));

            var taggedIds = webresourcesIdsArr.Select(a => $"<webresource>{a}</webresource>");
            var joinedTaggedIds = string.Join(Environment.NewLine, taggedIds);
            var request = new PublishXmlRequest
            {
                ParameterXml = $@"<importexportxml><webresources>{joinedTaggedIds}</webresources></importexportxml>"
            };

            return request;
        }


        public void CreateWebResource()
        {
            string publisherPrefix = _connectionDetail.PublisherPrefix;
            if (publisherPrefix == null)
            {
                var result = MessageBox.Show("Publisher prefix is not loaded. Do you want to load it from CRM?", "Prefix is missing", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    LoadPrefix();
                }
            }

            OpenCreateWebResourceForm();
        }


        private void OpenCreateWebResourceForm()
        {
            var path = ProjectHelper.GetSelectedFilePath();
            var dialog = new CreateWebResourceForm(path);
            dialog.ShowDialog();
        }

        private void LoadPrefix()
        {
            Logger.WriteLine("Retrieving Publisher prefix");
            var bwSolution = new BackgroundWorker();
            bwSolution.DoWork += BwGetSolutionDoWork;
            bwSolution.RunWorkerCompleted += BwGetSolutionRunWorkerCompleted;
            bwSolution.RunWorkerAsync();
        }

        private void BwGetSolutionRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                string errorMessage = e.Error.Message;
                var ex = e.Error.InnerException;
                while (ex != null)
                {
                    errorMessage += "\r\nInner Exception: " + ex.Message;
                    ex = ex.InnerException;
                }
                MessageBox.Show("An error occured while retrieving publisher prefix: " + errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (e.Result != null)
                {
                    var entity = e.Result as Entity;
                    string prefix = entity?.GetAttributeValue<AliasedValue>("publisher.customizationprefix")?.Value.ToString();
                    Logger.WriteLine("Publisher prefix successfully retrieved");
                    _connectionDetail.PublisherPrefix = prefix;
                    var settings = ProjectHelper.GetSettings();
                    settings.SelectedConnection.PublisherPrefix = prefix;
                    settings.Save();
                    OpenCreateWebResourceForm();
                }
            }
        }

        private void BwGetSolutionDoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = RetrieveSolution();
        }

        private Entity RetrieveSolution()
        {
            if (_connectionDetail.SolutionId == null)
            {
                return null;
            }

            var result = _orgService.GetSolution(_connectionDetail.SolutionId.Value);
            return result;
        }

        /// <summary>
        /// Shows Configuration Dialog
        /// </summary>
        /// <param name="mode">Configuration mode for settings dialog</param>
        /// <param name="project">Project to manage configuration for</param>
        /// <returns>Returns result of a configuration dialog</returns>
        public static DialogResult ShowConfigurationDialog(ConfigurationMode mode, Project project)
        {
            var settings = ProjectHelper.GetSettings();
            var crmConnections = settings.CrmConnections ?? new CrmConnections { Connections = new List<ConnectionDetail>() };
            var manager = new ConnectionManager
            {
                ConnectionsList = crmConnections
            };
            var selector = new ConnectionSelector(crmConnections, manager, settings.SelectedConnection, false, mode == ConfigurationMode.Update)
            {
                WorkingProject = project
            };
            selector.ShowDialog();
            settings.CrmConnections = selector.ConnectionList;
            if (selector.DialogResult == DialogResult.OK || selector.DialogResult == DialogResult.Yes)
            {
                settings.SelectedConnection = selector.SelectedConnection;
            }
            settings.Save();
            return selector.DialogResult;
        }
    }
}

