using System;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using McTools.Xrm.Connection;
using McTools.Xrm.Connection.WinForms;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using CrmWebResourcesUpdater.Helpers;
using CrmWebResourcesUpdater.OptionsForms.Properties;

namespace CrmWebResourcesUpdater.OptionsForms
{
    public partial class WebResourcesUpdaterForm : Form
    {
        #region Variables
        private readonly CrmConnectionStatusBar ccStatusBar;

        public ConnectionDetail SelectedConnection { get; private set; }
        public SolutionDetails SelectedSolution => comboBoxSolutions.SelectedItem as SolutionDetails;
        public bool AutoPublish
        {
            get => cbAutoPublish.Checked;
            set => cbAutoPublish.Checked = value;
        }
        public bool IgnoreExtensions
        {
            get => cbIgnoreExtensions.Checked;
            set => cbIgnoreExtensions.Checked = value;
        }
        public bool ExtendedLog
        {
            get => cbExtendedLog.Checked;
            set => cbExtendedLog.Checked = value;
        }

        private int connectionCount = 0;
        private readonly FormHelper formHelper;
        #endregion Variables

        #region Constructor
        public WebResourcesUpdaterForm()
        {
            InitializeComponent();
            // Create the connection manager with its events
            var manager = ConnectionManager.Instance;
            manager.FromXrmToolBox = false;
            manager.ConnectionSucceed += ConnectionManager_ConnectionSucceed;
            manager.ConnectionFailed += ConnectionManager_ConnectionFailed;
            manager.StepChanged += ConnectionManager_StepChanged;
            manager.RequestPassword += ConnectionManager_RequestPassword;
            formHelper = new FormHelper(this);

            // Instantiate and add the connection control to the form
            ccStatusBar = new CrmConnectionStatusBar(formHelper, tsbMergeConnectionsFiles.Checked)
            {
                BackgroundImageLayout = ImageLayout.None,
                BackColor = Color.Transparent,
                RenderMode = ToolStripRenderMode.System,
                Dock = DockStyle.Top,
                Location = new Point(3, 16),
                Name = "ConnectionStatusStrip",
                Size = new Size(555, 32),
                SizingGrip = false,
                TabStop = true,
                TabIndex = 0
            };
            groupBoxConnection.Controls.Add(ccStatusBar);
            comboBoxSolutions.Enabled = false;
            ccStatusBar.SetMessage("Please select a connection");

            this.Disposed += WebResourcesUpdaterForm_Disposed;
        }

        private void WebResourcesUpdaterForm_Disposed(object sender, EventArgs e)
        {
            var manager = ConnectionManager.Instance;
            manager.ConnectionSucceed -= ConnectionManager_ConnectionSucceed;
            manager.ConnectionFailed -= ConnectionManager_ConnectionFailed;
            manager.StepChanged -= ConnectionManager_StepChanged;
            manager.RequestPassword -= ConnectionManager_RequestPassword;
        }

        public void Init(Settings settings)
        {
            if (settings == null)
                return;

            AutoPublish = settings.AutoPublish;
            IgnoreExtensions = settings.IgnoreExtensions;
            ExtendedLog = settings.ExtendedLog;
            
            Log("Init Connection");
            ConnectionDetail connection = settings.Connection;
            if (connection != null)
            {
                if (connection != SelectedConnection)
                {
                    settings.Connection = InitConnection(connection);
                    ClearSolutions();
                }
            }
            else
            {
                SelectedConnection = null;
                ClearSolutions();
            }

            Log("Init Solution");
            var solution = settings.Solution;
            if (solution != null)
            {
                if (solution != SelectedSolution)
                {
                    solution = InitSolution(solution);
                    settings.Solution = solution;
                    if (solution == null)
                        ResetSolution();
                }
            }
            else 
                ResetSolution();

            Log("Init Success");
        }

        private void ResetSolution()
        {
            comboBoxSolutions.SelectedIndex = -1;
        }

        private void ClearSolutions()
        {
            comboBoxSolutions.Items.Clear();
            comboBoxSolutions.Enabled = false;
        }

        private ConnectionDetail InitConnection(ConnectionDetail connection)
        {
            Log("Saved Connection: " + connection);
            try
            {
                var manager = ConnectionManager.Instance;
                var crmConnections = manager.LoadConnectionsList();
                var loadedConnection = crmConnections.Connections.Find(c => c.ConnectionId == connection.ConnectionId);
                if (loadedConnection == null)
                {
                    manager.ConnectionsList.Connections.Add(connection);
                    manager.SaveConnectionsFile();
                }
                else
                    connection = loadedConnection;

                //Reconnect on Open Dialog
                //ConnectTo(connection);

                // Do not reconnect
                SetConnectionStatus(connection);
            }
            catch (Exception e)
            {
                Log("Init Fail");
                Log(e.ToString());
                return null;
            }

            return connection;
        }

        private SolutionDetails InitSolution(SolutionDetails solution)
        {
            if (comboBoxSolutions.Items.Count == 0)
            {
                comboBoxSolutions.Items.Add(solution);
                comboBoxSolutions.SelectedIndex = 0;
                return solution;
            }

            var solutions = comboBoxSolutions.Items.Cast<SolutionDetails>().ToList();
            int selectedIndex = solutions.FindIndex(s => s.SolutionId == solution.SolutionId);
            if (selectedIndex > 0)
            {
                comboBoxSolutions.SelectedIndex = selectedIndex;
                solution = solutions[selectedIndex];
                return solution;
            }
            return null;
        }

        private bool ConnectionManager_RequestPassword(object sender, RequestPasswordEventArgs e)
        {
            return formHelper.RequestPassword(e.ConnectionDetail);
        }
        #endregion Constructor

        #region Connection event handlers

        /// <summary>
        /// Occurs when the connection to a server failed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectionManager_ConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            Log("Error: " + e.FailureReason);
            ccStatusBar.SetMessage("Error: " + e.FailureReason);
            SelectedConnection = null;
        }

        /// <summary>
        /// Occurs when the connection to a server succeed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectionManager_ConnectionSucceed(object sender, ConnectionSucceedEventArgs e)
        {
            connectionCount++;
            var connection = e.ConnectionDetail;
            
            SetConnectionStatus(connection);

            var connectionsRequested = e.NumberOfConnectionsRequested;
            if (connectionCount == connectionsRequested && connectionsRequested > 1)
                Log("All connections done!");

            var parameter = e.Parameter;
            if (parameter is Action<IOrganizationService> action)
                action.Invoke(e.OrganizationService);
        }

        private void SetConnectionStatus(ConnectionDetail connection)
        {
            ccStatusBar.RebuildConnectionList();

            Log("SelectedConnection is set to: " + connection);
            SelectedConnection = connection;

            // Displays connection status
            ccStatusBar.SetConnectionStatus(true, connection);

            // Clear the current action message
            ccStatusBar.SetMessage(string.Empty);
        }

        #region WhoAmI Sample methods
        private void btnWhoAmI_Click(object sender, EventArgs e)
        {
            ConnectAndExecute(WhoAmI);
        }
        
        public void ConnectAndExecute(Action<IOrganizationService> action)
        {
            Log($"Connect and Execute '{action.Target}: {action.GetHashCode()}'");

            var connection = SelectedConnection;
            formHelper.AskForConnection(connection, action, () => Log($"Connection requested to {connection.ConnectionName}"));
        }

        private void WhoAmI(IOrganizationService service)
        {
            Log("Executing WhoAmI ...");
            var query = new QueryExpression("systemuser")
            {
                NoLock = true,
                ColumnSet = new ColumnSet("fullname")
            };
            query.Criteria.AddCondition("systemuserid", ConditionOperator.EqualUserId);
            var queryResult = service.RetrieveMultiple(query);
            var user = queryResult.Entities.First();

            Log($"Hello {user.GetAttributeValue<string>("fullname")},Your ID is: {user.Id:B}");
        }
        #endregion WhoAmI Sample methods

        /// <summary>
        /// Occurs when the connection manager sends a step change
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectionManager_StepChanged(object sender, StepChangedEventArgs e)
        {
            ccStatusBar.SetMessage(e.CurrentStep);
            Log(e.CurrentStep);
        }

        #endregion Connection event handlers

        private void tsbManageConnections_Click(object sender, EventArgs e)
        {
            try
            {
                formHelper.DisplayConnectionsList(this);
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        private void tsbMergeConnectionsFiles_Click(object sender, EventArgs e)
        {
            ccStatusBar.MergeConnectionsFiles = tsbMergeConnectionsFiles.Checked;
        }

        private void bCreateMapping_Click(object sender, EventArgs e)
        {
            try
            {
                Log("Creating Mapping File ...");
                string filePath = MappingHelper.CreateMappingFile();
                if (filePath == null)
                    Log("Creating Mapping File Fail");
                Log(filePath + " " + Resources.Successfully_Created);
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        private void bGetSolutions_Click(object sender, EventArgs e)
        {
            ConnectAndExecute(GetSolutions);
        }

        private void GetSolutions(IOrganizationService service)
        {
            Cursor = Cursors.WaitCursor;

            WebRequest.GetSystemWebProxy();

            var query = new QueryExpression
            {
                EntityName = "solution",
                ColumnSet = new ColumnSet("friendlyname", "uniquename", "publisherid"),
                Criteria = new FilterExpression()
            };
            query.Criteria.AddCondition("isvisible", ConditionOperator.Equal, true);

            query.LinkEntities.Add(new LinkEntity("solution", "publisher", "publisherid", "publisherid", JoinOperator.Inner));
            query.LinkEntities[0].Columns.AddColumns("customizationprefix");
            query.LinkEntities[0].EntityAlias = "publisher";
            query.AddOrder("friendlyname", OrderType.Ascending);

            var response = service.RetrieveMultiple(query);

            var solutionId = SelectedSolution?.SolutionId;
            ComboBox.ObjectCollection items = comboBoxSolutions.Items;
            items.Clear();

            int i = 0;
            int selectedIndex = 0;

            var entities = response.Entities;
            foreach (Entity entity in entities)
            {
                var solutionDetails = new SolutionDetails
                {
                    SolutionId = entity.Id,
                    UniqueName = entity.GetAttributeValue<string>("uniquename"),
                    FriendlyName = entity.GetAttributeValue<string>("friendlyname"),
                    PublisherPrefix = entity.GetAttributeValue<AliasedValue>("publisher.customizationprefix") == null ? null : entity.GetAttributeValue<AliasedValue>("publisher.customizationprefix").Value.ToString()
                };

                items.Add(solutionDetails);
                if (selectedIndex == 0 && solutionDetails.SolutionId == solutionId)
                    selectedIndex = i;

                i++;
            }

            comboBoxSolutions.Enabled = items.Count > 1;
            comboBoxSolutions.SelectedIndex = selectedIndex;

            Cursor = Cursors.Default;
        }

        public void Log(string message)
        {
            tbLogs.AppendText(message + Environment.NewLine);
        }

        private void tsbClearLogs_Click(object sender, EventArgs e)
        {
            tbLogs.Clear();
        }

        private void bSave_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void bCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

    }
}