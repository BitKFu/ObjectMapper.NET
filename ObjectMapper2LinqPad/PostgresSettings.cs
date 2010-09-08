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
    public partial class PostgresSettings : UserControl
    {
        public PostgresSettings()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes the specified connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public void Initialize(LinqPadConnection connection)
        {
            txtDatabase.Text = connection.DatabaseName;
            txtPassword.Text = connection.Password;
            txtServer.Text = connection.ServerName;
            txtUserName.Text = connection.UserName;
            chkLowerCasing.Checked = connection.SqlCasing == SqlCasing.LowerCase;
        }

        public void ValidateAndApplySettings(LinqPadConnection connection)
        {
            connection.DatabaseType = DatabaseType.Postgres;

            connection.UserName = txtUserName.Text;
            connection.Password = txtPassword.Text;
            connection.DatabaseName = txtDatabase.Text;
            connection.ServerName = txtServer.Text;
            connection.SqlCasing = chkLowerCasing.Checked ? SqlCasing.LowerCase : SqlCasing.Mixed;

            using (OBM.CreateMapper(connection))
            {
                // Try to create a connection
            }
        }
    }
}
