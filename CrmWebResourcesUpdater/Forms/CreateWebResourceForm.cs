using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using CrmWebResourcesUpdater.Helpers;
using CrmWebResourcesUpdater.Common;
using CrmWebResourcesUpdater.OptionsForms;

namespace CrmWebResourcesUpdater.Forms
{
    public partial class CreateWebResourceForm : Form
    {

        public string ProjectItemPath { get; set; }
        private readonly IOrganizationService service;
        private readonly Settings settings;

        public CreateWebResourceForm(IOrganizationService service, string filePath)
        {
            settings = ProjectHelper.GetSettings<Settings>();
            if (settings.Solution.SolutionId == null)
            {
                throw new ArgumentNullException("SolutionId");
            }

            WebRequest.GetSystemWebProxy();  // Todo: require this?
            this.service = service;

            ProjectItemPath = filePath;
            InitializeComponent();
        }

        private void bCreate_Click(object sender, EventArgs e)
        {
            var name = tbName.Text;
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Name can not be empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var prefix = tbPrefix.Text;
            if (string.IsNullOrEmpty(prefix))
            {
                MessageBox.Show("Prefix can not be empty", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (cbType.SelectedIndex < 0)
            {
                MessageBox.Show("Please select web resource type", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var webresourceName = prefix + "_" + name;

            var webResource = new Entity
            {
                ["name"] = webresourceName,
                ["displayname"] = tbDisplayName.Text,
                ["description"] = tbDescription.Text,
                ["content"] = ProjectHelper.GetEncodedFileContent(ProjectItemPath),
                ["webresourcetype"] = new OptionSetValue(cbType.SelectedIndex + 1),
                LogicalName = "webresource"
            };


            Cursor.Current = Cursors.WaitCursor;
            var project = ProjectHelper.GetSelectedProject();
            if (IsResourceExists(webresourceName))
            {
                MessageBox.Show("Webresource with name '" + webresourceName + "' already exist in CRM.", "Webresource already exists.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                Cursor.Current = Cursors.Arrow;
                var isMappingRequired = MappingHelper.IsMappingRequired(project, ProjectItemPath, webresourceName);
                var isMappingFileReadOnly = MappingHelper.IsMappingFileReadOnly(project);
                if (isMappingRequired && isMappingFileReadOnly)
                {
                    var message = "Mapping record can't be created. File \"UploaderMapping.config\" is read-only. Do you want to proceed? \r\n\r\n" +
                                    "Schema name of the web resource you are creating is differ from the file name. " +
                                    "Because of that new mapping record has to be created in the file \"UploaderMapping.config\". " +
                                    "Unfortunately the file \"UploaderMapping.config\" is read-only (file might be under a source control), so mapping record cant be created. \r\n\r\n" +
                                    "Press OK to proceed without mapping record creation (You have to do that manually later). Press Cancel to fix problem and try later.";
                    var result = MessageBox.Show(message, "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                    if (result == DialogResult.Cancel)
                    {
                        return;
                    }
                }
                if (isMappingRequired && !isMappingFileReadOnly)
                {
                    MappingHelper.CreateMapping(ProjectItemPath, webresourceName, project);
                }
                CreateWebResource(webResource);
                Logger.WriteLine("Webresource '" + webresourceName + "' was successfully created");
                Close();
            }
            catch (Exception ex)
            {
                Cursor.Current = Cursors.Arrow;
                MessageBox.Show("An error occured during web resource creation: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateWebResource(Entity webResource)
        {
            if (webResource == null)
            {
                throw new ArgumentNullException("Web resource can not be null");
            }
            var createRequest = new CreateRequest
            {
                Target = webResource
            };
            createRequest.Parameters.Add("SolutionUniqueName", settings.Solution.UniqueName);
            service.Execute(createRequest);
        }

        private bool IsResourceExists(string webResourceName)
        {
            var query = new QueryExpression
            {
                EntityName = "webresource",
                ColumnSet = new ColumnSet("name")
            };
            query.Criteria.AddCondition("name", ConditionOperator.Equal, webResourceName);

            var response = service.RetrieveMultiple(query);
            var entity = response.Entities.FirstOrDefault();

            return entity != null;
        }

        private void CreateWebResourceFormLoad(object sender, EventArgs e)
        {
            var prefix = settings.Solution.PublisherPrefix ?? "";
            var name = Path.GetFileName(ProjectItemPath);
            var extension = Path.GetExtension(ProjectItemPath);
            extension = extension?.Substring(1);

            var re = new Regex("^" + prefix + "_");
            name = re.Replace(name, "");

            tbPrefix.Text = prefix;
            tbName.Text = name;
            tbDisplayName.Text = $@"{prefix}_{name}";
            tbDescription.Text = "";

            var items = new[] {
                 new WebResourceType(0 , "Webpage (HTML)", "htm", "html"),
                 new WebResourceType(1, "Stylesheet (CSS)", "css"),
                 new WebResourceType(2, "Script (JScript)", "js"),
                 new WebResourceType(3, "Data (XML)", "xml"),
                 new WebResourceType(4, "Image (PNG)", "png"),
                 new WebResourceType(5, "Image (JPG)", "jpg", "jpeg"),
                 new WebResourceType(6, "Image (GIF)", "gif"),
                 new WebResourceType(7, "Silverlight (XAP)", "xap"),
                 new WebResourceType(8, "Stylesheet (XSL)", "xsl"),
                 new WebResourceType(9, "Image (ICO)", "ico")
            };
            cbType.Items.AddRange(items);
            cbType.SelectedItem = items.FirstOrDefault(i => i.Extensions.Contains(extension, StringComparer.OrdinalIgnoreCase));
        }
    }
}
