using McTools.Xrm.Connection;
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

namespace CrmWebResourcesUpdater.Forms
{
    public partial class CreateWebResourceForm : Form
    {
        public Entity WebResource { get; set; }
        public string ProjectItemPath {get; set;}
        private IOrganizationService _service;
        private ConnectionDetail _connectionDetail;

        private const string FileKindGuid = "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";
        private const string MappingFileName = "UploaderMapping.config";


        public CreateWebResourceForm(string filePath)
        {
            var settings = ProjectHelper.GetSettings();
            _connectionDetail = settings.SelectedConnection;
            if (_connectionDetail.SolutionId == null)
            {
                throw new ArgumentNullException("SolutionId");
            }
            WebRequest.GetSystemWebProxy();
            _service = CrmConnectionHelper.GetOrganizationServiceProxy(_connectionDetail);

            ProjectItemPath = filePath;
            InitializeComponent();
        }

        private void bCreateClick(object sender, EventArgs e)
        {
            var name = tbName.Text;
            if(string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Name can not be empty", "Error",MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            WebResource = new Entity();

            WebResource["name"] = webresourceName;
            WebResource["displayname"] = tbDisplayName.Text;
            WebResource["description"] = tbDescription.Text;
            WebResource["content"] = ProjectHelper.GetEncodedFileContent(ProjectItemPath);
            WebResource["webresourcetype"] = new OptionSetValue(cbType.SelectedIndex + 1);
            WebResource.LogicalName = "webresource";

            Cursor.Current = Cursors.WaitCursor;
            var project = ProjectHelper.GetSelectedProject();
            if (isResourceExists(webresourceName))
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
                    if(result == DialogResult.Cancel)
                    {
                        return;
                    }
                }
                if (isMappingRequired && !isMappingFileReadOnly)
                {
                    MappingHelper.CreateMapping(project, ProjectItemPath, webresourceName);
                }
                CreateWebResource(WebResource);
                Logger.WriteLine("Webresource '" + webresourceName + "' was successfully created");
                Close();
            }
            catch(Exception ex)
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
            CreateRequest createRequest = new CreateRequest
            {
                Target = webResource
            };
            createRequest.Parameters.Add("SolutionUniqueName", _connectionDetail.Solution);
            _service.Execute(createRequest);
        }

        private bool isResourceExists(string webResourceName)
        {
            QueryExpression query = new QueryExpression
            {
                EntityName = "webresource",
                ColumnSet = new ColumnSet(new string[] { "name" }),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("name", ConditionOperator.Equal, webResourceName);
            //query.Criteria.AddCondition("solutionid", ConditionOperator.Equal, _connectionDetail.SolutionId);

            var response = _service.RetrieveMultiple(query);
            var entity = response.Entities.FirstOrDefault();

            return entity == null ? false : true;
        }

        

        private void bCancelClick(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void CreateWebResourceFormLoad(object sender, EventArgs e)
        {
            var prefix = _connectionDetail.PublisherPrefix == null ? "" : _connectionDetail.PublisherPrefix;
            var name = Path.GetFileName(ProjectItemPath);
            var extension = Path.GetExtension(ProjectItemPath).ToLower();

            var re = new Regex("^" + prefix + "_");
            name = re.Replace(name, "");
            

            tbPrefix.Text = prefix;
            tbName.Text = name;
            tbDisplayName.Text = prefix + "_" + name;
            tbDescription.Text = "";

            cbType.Items.Add("Webpage (HTML)");
            cbType.Items.Add("Stylesheet (CSS)");
            cbType.Items.Add("Script (JScript)");
            cbType.Items.Add("Data (XML)");
            cbType.Items.Add("Image (PNG)");
            cbType.Items.Add("Image (JPG)");
            cbType.Items.Add("Image (GIF)");
            cbType.Items.Add("Silverlight (XAP)");
            cbType.Items.Add("Stylesheet (XSL)");
            cbType.Items.Add("Image (ICO)");
            switch(extension)
            {
                case ".htm":
                case ".html": { cbType.SelectedIndex = 0; break; }
                case ".css": { cbType.SelectedIndex = 1; break; }
                case ".js": { cbType.SelectedIndex = 2; break; }
                case ".xml": { cbType.SelectedIndex = 3; break; }
                case ".png": { cbType.SelectedIndex = 4; break; }
                case ".jpg":
                case ".jpeg": { cbType.SelectedIndex = 5; break; }
                case ".gif": { cbType.SelectedIndex = 6; break; }
                case ".xap": { cbType.SelectedIndex = 7; break; }
                case ".xsl": { cbType.SelectedIndex = 8; break; }
                case ".ico": { cbType.SelectedIndex = 9; break; }
                default: { cbType.SelectedIndex = -1; break; }
            }
            
        }
    }
}
