using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AdFactum.Data.Internal;
using AdFactum.Data.Util;

namespace ObjectMapper2LinqPad
{
    public partial class SqlServerSettings : UserControl
    {
        public SqlServerSettings()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called when [checked changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnCheckedChanged(object sender, EventArgs e)
        {
            txtUserName.Enabled = txtPassword.Enabled = !chkTrustedConnection.Checked;
        }

        /// <summary>
        /// Initializes the specified connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public void Initialize(LinqPadConnection connection)
        {
            txtUserName.Text = connection.UserName;
            txtPassword.Text = connection.Password;
            chkTrustedConnection.Checked = connection.TrustedConnection;
            txtDatabase.Text = connection.DatabaseName;
            txtServer.Text = connection.ServerName;
        }

        /// <summary>
        /// Validates the and apply settings.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public void ValidateAndApplySettings(LinqPadConnection connection)
        {
            connection.DatabaseType = DatabaseType.SqlServer;

            connection.UserName = txtUserName.Text;
            connection.Password = txtPassword.Text;
            connection.TrustedConnection = chkTrustedConnection.Checked;
            connection.DatabaseName = txtDatabase.Text;
            connection.ServerName = txtServer.Text;
            connection.SqlCasing = SqlCasing.Mixed;

            using (OBM.CreateMapper(connection))
            {
                // Try to create a connection
            }
        }
    }
}
