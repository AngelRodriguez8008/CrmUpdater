using System;
using System.Windows.Forms;

namespace McTools.Xrm.Connection.WinForms
{
    /// <summary>
    /// Formulaire Windows permettant de demander le mot de 
    /// passe d'un utilisateur
    /// </summary>
    public partial class PasswordForm : Form
    {
        #region Variables

        /// <summary>
        /// Login de l'utilisateur
        /// </summary>
        string userLogin;

        /// <summary>
        /// Nom de domaine pour l'utilisateur
        /// </summary>
        string userDomain;

        /// <summary>
        /// Mot de passe de l'utilisateur
        /// </summary>
        string userPassword;

        #endregion

        #region Constructeur

        /// <summary>
        /// Créé une nouvelle instance de la classe PasswordForm
        /// </summary>
        public PasswordForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Propriétés

        /// <summary>
        /// Obtient ou définit le login de l'utilisateur
        /// </summary>
        public string UserLogin
        {
            get => userLogin;
            set => userLogin = value;
        }

        /// <summary>
        /// Obtient ou définit le nom de domaine pour l'utilisateur
        /// </summary>
        public string UserDomain
        {
            get => userDomain;
            set => userDomain = value;
        }

        /// <summary>
        /// Obtient le mot de passe de l'utilisateur
        /// </summary>
        public string UserPassword => userPassword;

        public bool SavePassword { get; set; }

        #endregion

        #region Méthodes

        protected override void OnLoad(EventArgs e)
        {
            tbUserLogin.Text = $"{userDomain}{(userDomain.Length > 0 ? "\\" : "")}{userLogin}";

            base.OnLoad(e);
        }

        private void bValidate_Click(object sender, EventArgs e)
        {
            bool go = true;

            if (tbPassword.Text.Length == 0)
            {
                if (MessageBox.Show(this, "Are you sure you want to leave the password empty?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    go = false;
                }
            }

            if (go)
            {
                userPassword = tbPassword.Text;
                SavePassword = chkSavePassword.Checked;
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void bCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void tbPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                bValidate_Click(null, null);
            }
        }

        private void chkShowCharacters_CheckedChanged(object sender, EventArgs e)
        {
            tbPassword.PasswordChar = chkShowCharacters.Checked ? (char)0 : '•';
        }

        #endregion
    }
}