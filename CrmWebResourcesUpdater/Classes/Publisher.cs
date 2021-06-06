using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xrm.Sdk.Query;
using CrmWebResourcesUpdater.Forms;
using System.Windows.Forms;
using CrmWebResourcesUpdater.Common;
using CrmWebResourcesUpdater.Helpers;
using CrmWebResourcesUpdater.OptionsForms;
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


        private Settings _settings;
        private Settings Settings => _settings ?? (_settings = GetSettings());

        private Settings GetSettings()
        {
            var result = ProjectHelper.GetSettings<Settings>();

            if (result?.Connection != null)
                return result;

            if (ProjectHelper.ShowErrorDialog() == DialogResult.Yes)
                result = ShowConfigurationDialog(result);

            return result;
        }

        /// <summary>
        /// Uploads and publishes files to CRM
        /// </summary>
        public void PublishWebResources(bool uploadSelectedItems = false)
        {
            ProjectHelper.SaveAll();
            Logger.Clear();

            Settings settings = this.Settings;
            if (settings?.Connection == null)
            {
                Logger.WriteLine("Error: Connection is not selected");
                return;
            }
            
            if (settings.Solution?.SolutionId == null)
            {
                Logger.WriteLine("Error: Solution is not selected");
                return;
            }

            ProjectHelper.SetStatusBar("Uploading...");
            Logger.WriteLineWithTime(settings.AutoPublish ? "Publishing web resources..." : "Uploading web resources...");

            Logger.WriteLine("Connecting to CRM...");
            Logger.WriteLine("URL: " + settings.Connection.WebApplicationUrl);
            Logger.WriteLine("Solution Name: " + settings.Solution.FriendlyName);
            Logger.WriteLine("--------------------------------------------------------------");

            Logger.WriteLine("Loading files paths", settings.ExtendedLog);
            var projectFiles = GetSelectedFiles(uploadSelectedItems);
            
            if (projectFiles == null || projectFiles.Count == 0)
            {
                Logger.WriteLine("Failed to load files paths", settings.ExtendedLog);
                return;
            }

            Logger.WriteLine(projectFiles.Count + " path" + (projectFiles.Count == 1 ? " was" : "s were") + " loaded", settings.ExtendedLog);

            Execute(settings, service => TryPublishWebResources(service, projectFiles));
        }

        private void TryPublishWebResources( IOrganizationService service, List<string> projectFiles)
        {
            try
            {
                Logger.WriteLine("Starting uploading process", Settings.ExtendedLog);
                var webresources = UploadWebResources(service, projectFiles);
                Logger.WriteLine("Uploading process was finished", Settings.ExtendedLog);

                if (webresources.Count > 0)
                {
                    Logger.WriteLine("--------------------------------------------------------------");
                    foreach (var name in webresources.Values)
                        Logger.WriteLine(name + " successfully uploaded");

                    Logger.WriteLine("Updating Solution Version ...");
                    var solutionVersion = VersionsHelper.UpdateSolution(service, Settings.Solution.SolutionId);
                    Logger.WriteLine($"New Solution Version: {solutionVersion} ");
                }

                Logger.WriteLine("--------------------------------------------------------------");
                var message = webresources.Count + " file" + (webresources.Count == 1 ? " was" : "s were");
                Logger.WriteLineWithTime(message + " uploaded");


                if (Settings.AutoPublish)
                {
                    ProjectHelper.SetStatusBar("Publishing...");
                    PublishWebResources(service, webresources.Keys);

                    ProjectHelper.SetStatusBar(message + " published");
                }
                else
                {
                    ProjectHelper.SetStatusBar(message + " uploaded");
                }
            }
            catch (Exception ex)
            {
                ProjectHelper.SetStatusBar("Failed to publish script" + (projectFiles.Count == 1 ? "" : "s"));
                Logger.WriteLine("Failed to publish script" + (projectFiles.Count == 1 ? "." : "s."));
                Logger.WriteLine(ex.Message);
                Logger.WriteLine(ex.StackTrace, Settings.ExtendedLog);
            }

            Logger.WriteLineWithTime("Done.");
        }


        public void Dispose()
        {
            _settings?.Connection?.ServiceClient?.Dispose();
        }

        public List<string> GetSelectedFiles(bool uploadSelectedItems = false)
        {
            List<string> projectFiles;
            if (uploadSelectedItems)
            {
                Logger.WriteLine("Loading selected file's paths", Settings.ExtendedLog);
                projectFiles = ProjectHelper.GetSelectedFiles();
            }
            else
            {
                Logger.WriteLine("Loading all files' paths", Settings.ExtendedLog);
                projectFiles = ProjectHelper.GetProjectFiles();
            }

            return projectFiles;
        }

        /// <summary>
        /// Uploads web resources
        /// </summary>
        /// <param name="service">Organization Service Client</param>
        /// <param name="projectFiles"></param>
        /// <returns>List of Guids of web resources that was updateds</returns>            
        private Dictionary<Guid, string> UploadWebResources( IOrganizationService service, List<string> projectFiles = null)
        {
            projectFiles = projectFiles ?? GetSelectedFiles();
            if (projectFiles == null || projectFiles.Count == 0)
            {
                return null;
            }

            var ids = new Dictionary<Guid, string>();
            var project = ProjectHelper.GetSelectedProject();
            var projectRootPath = ProjectHelper.GetProjectRoot(project);
            var mappings = MappingHelper.LoadMappings(project);
            var webResources = RetrieveWebResources(service);

            foreach (var filePath in projectFiles)
            {
                var webResourceName = Path.GetFileName(filePath);
                var lowerFilePath = filePath.ToLower();

                if (string.Equals(webResourceName, MappingHelper.MappingFileName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (mappings != null && mappings.ContainsKey(lowerFilePath))
                {
                    webResourceName = mappings[lowerFilePath];
                    var relativePath = lowerFilePath.Replace(projectRootPath + "\\", "");
                    Logger.WriteLine("Mapping found: " + relativePath + " to " + webResourceName, Settings.ExtendedLog);
                }

                var webResource = webResources.FirstOrDefault(x => x.GetAttributeValue<string>("name") == webResourceName);
                if (webResource == null && Settings.IgnoreExtensions)
                {
                    Logger.WriteLine(webResourceName + " does not exists in selected solution", Settings.ExtendedLog);
                    webResourceName = Path.GetFileNameWithoutExtension(filePath);
                    Logger.WriteLine("Searching for " + webResourceName, Settings.ExtendedLog);
                    webResource = webResources.FirstOrDefault(x => x.GetAttributeValue<string>("name") == webResourceName);
                }
                if (webResource == null)
                {
                    Logger.WriteLine("Uploading of " + webResourceName + " was skipped: web resource does not exists in selected solution", Settings.ExtendedLog);
                    Logger.WriteLine(webResourceName + " does not exists in selected solution", !Settings.ExtendedLog);
                    continue;
                }
                if (!File.Exists(lowerFilePath))
                {
                    Logger.WriteLine("Warning: File not found: " + lowerFilePath);
                    continue;
                }
                var isUpdated = UpdateWebResourceByFile(service, webResource, filePath);
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
        /// <param name="service"></param>
        /// <param name="webResource">Web resource to be updated</param>
        /// <param name="filePath">File with a content to be set for web resource</param>
        /// <returns>Returns true if web resource is updated</returns>
        private bool UpdateWebResourceByFile(IOrganizationService service, Entity webResource, string filePath)
        {
            var webResourceName = Path.GetFileName(filePath);
            Logger.WriteLine("Uploading " + webResourceName, Settings.ExtendedLog);

            var project = ProjectHelper.GetSelectedProject();
            var projectRootPath = ProjectHelper.GetProjectRoot(project);

            var localContent = ProjectHelper.GetEncodedFileContent(filePath);
            var remoteContent = webResource.GetAttributeValue<string>("content");

            var hasContentChanged = remoteContent.Length != localContent.Length || remoteContent != localContent;
            if (hasContentChanged == false)
            {
                Logger.WriteLine("Uploading of " + webResourceName + " was skipped: there aren't any change in the web resource", Settings.ExtendedLog);
                Logger.WriteLine(webResourceName + " has no changes", !Settings.ExtendedLog);
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

            UpdateWebResourceByContent(service, webResource, localContent);
            var relativePath = filePath.Replace(projectRootPath + "\\", "");
            webResourceName = webResource.GetAttributeValue<string>("name");
            Logger.WriteLine($"{webResourceName} uploaded from {relativePath}{version}", !Settings.ExtendedLog);
            return true;
        }

        /// <summary>
        /// Uploads web resource
        /// </summary>
        /// <param name="service">Organization Service Client</param>
        /// <param name="webResource">Web resource to be updated</param>
        /// <param name="content">Content to be set for web resource</param>
        private void UpdateWebResourceByContent(IOrganizationService service, Entity webResource, string content)
        {
            var name = webResource.GetAttributeValue<string>("name");
            webResource["content"] = content;
            service.Update(webResource);

            Logger.WriteLine(name + " was successfully uploaded", Settings.ExtendedLog);
        }

        /// <summary>
        /// Retrieves web resources for selected items
        /// </summary>
        /// <returns>List of web resources</returns>
        private List<Entity> RetrieveWebResources(IOrganizationService service)
        {
            Logger.WriteLine("Retrieving existing web resources", Settings.ExtendedLog);
            var solutionId = Settings.Solution.SolutionId;

            var fetchQuery = string.Format(FetchWebResourcesQueryTemplate, solutionId);
            var query = new FetchExpression(fetchQuery);
            var response = service.RetrieveMultiple(query);
            var webResources = response.Entities.ToList();

            return webResources;
        }

        /// <summary>
        /// Publish webresources changes
        /// </summary>
        /// <param name="service">Organization Service Client</param>
        /// <param name="webresourcesIds">List of webresource IDs to publish</param>
        private void PublishWebResources( IOrganizationService service, IEnumerable<Guid> webresourcesIds)
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
                service.Execute(request);
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
            var settings = this.Settings;
            if (settings?.Connection == null)
            {
                Logger.WriteLine("Error: Connection is not selected");
                return;
            }
            
            if (settings.Solution?.SolutionId == null)
            {
                Logger.WriteLine("Error: Solution is not selected");
                return;
            }
            Execute(settings, OpenCreateWebResourceForm, "An error occurred while creating WebResource");
        }

        private void OpenCreateWebResourceForm(IOrganizationService service)
        {
            string publisherPrefix = Settings.Solution.PublisherPrefix;
            if (publisherPrefix == null)
            {
                var result = MessageBox.Show(
                    "Publisher prefix is not loaded. Do you want to load it from CRM?",
                    "Prefix is missing",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                    return;

                LoadPrefix(service);
            }

            var path = ProjectHelper.GetSelectedFilePath();
            var dialog = new CreateWebResourceForm(service, path){
                StartPosition = FormStartPosition.CenterParent
            };
            dialog.ShowDialog();
        }

        private void LoadPrefix(IOrganizationService service)
        {
            var settings = Settings;
            var solutionId = settings.Solution?.SolutionId;
            if (solutionId == null)
                return;

            Logger.WriteLine("Retrieving Publisher prefix");
            var entity = service.GetSolution(solutionId.Value);

            if (entity == null)
                return;

            string prefix = entity.GetAttributeValue<AliasedValue>("publisher.customizationprefix")?.Value.ToString();
            Logger.WriteLine("Publisher prefix successfully retrieved");

            settings.Solution.PublisherPrefix = prefix;
            ProjectHelper.SaveSettings(settings);
        }
        
        static WebResourcesUpdaterForm optionsForm;

        /// <summary>
        /// Shows Configuration Dialog
        /// </summary>
        /// <param name="settings"></param>
        /// <returns>Returns result of a configuration dialog</returns>
        public Settings ShowConfigurationDialog(Settings settings = null)
        {
            if (optionsForm == null)
                optionsForm = new WebResourcesUpdaterForm {StartPosition = FormStartPosition.CenterParent};

            if (settings == null)
                settings = ProjectHelper.GetSettings<Settings>();

            if (settings != null)
                optionsForm.Init(settings);
            else
                settings = new Settings();

            optionsForm.ShowDialog();

            if (optionsForm.DialogResult == DialogResult.OK)
            {
                settings.Connection = optionsForm.SelectedConnection;
                settings.Solution = optionsForm.SelectedSolution;
                settings.AutoPublish = optionsForm.AutoPublish;
                settings.IgnoreExtensions = optionsForm.IgnoreExtensions;
                settings.ExtendedLog = optionsForm.ExtendedLog;

                ProjectHelper.SaveSettings(settings);
            }
            return settings;
        }
        
        public void Execute(Settings settings, Action<IOrganizationService> serviceAction, string errorMessage = null)
        {
            try
            {
                if(optionsForm == null)
                {
                    Logger.WriteLine("Init Configuration Options Form");
                    optionsForm = new WebResourcesUpdaterForm {
                        StartPosition = FormStartPosition.CenterParent
                    };
                }
                optionsForm.Init(settings);
                optionsForm.ConnectAndExecute(serviceAction);
            }
            catch (Exception e)
            {
                LogError("Error executing service action", e);
            }
        }

        private static void LogError(string message, Exception error = null)
        {
            var errorMessage = error?.GetErrorMessage();
            Logger.WriteLine(string.Join(Environment.NewLine, message, errorMessage));
        }
    }
}

