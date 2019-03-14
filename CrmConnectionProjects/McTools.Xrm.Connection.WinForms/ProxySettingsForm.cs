using System;
using System.Windows.Forms;

namespace McTools.Xrm.Connection.WinForms
{
    public partial class ProxySettingsForm : Form
    {
        public string proxyAddress { get; set; }
        public string proxyPort { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool UseCustomProxy { get; set; }

        public ProxySettingsForm()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            chkCustomProxy.Checked = UseCustomProxy;
            txtProxyAddress.Text = proxyAddress ?? string.Empty;
            txtProxyPort.Text = proxyPort ?? string.Empty;
            txtUserLogin.Text = UserName ?? string.Empty;
            txtUserPassword.Text = Password ?? string.Empty;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            proxyAddress = txtProxyAddress.Text;
            proxyPort = txtProxyPort.Text;
            UserName = txtUserLogin.Text;
            Password = txtUserPassword.Text;
            UseCustomProxy = chkCustomProxy.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            UseCustomProxy = chkCustomProxy.Checked;

            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void chkCustomProxy_CheckedChanged(object sender, EventArgs e)
        {
            pnlCustomSettings.Enabled = chkCustomProxy.Checked;
        }

        private void txtProxyPort_TextChanged(object sender, EventArgs e)
        {
            int port;

            if (!int.TryParse(txtProxyPort.Text, out port))
            {
                MessageBox.Show(this, "Only numeric characters are allowed!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProxyPort.Text = txtProxyPort.Text.Substring(0, txtProxyPort.Text.Length - 1);
                txtProxyPort.Select(txtProxyPort.Text.Length, 0);
            }
        }
    }
}
