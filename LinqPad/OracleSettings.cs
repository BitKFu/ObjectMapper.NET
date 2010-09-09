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
    public partial class OracleSettings : UserControl
    {
        public OracleSettings()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes the specified connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public void Initialize(LinqPadConnection connection)
        {
            txtUserName.Text = connection.UserName;
            txtPassword.Text = connection.Password;
            txtDatabaseAlias.Text = connection.DbAlias;
        }

        public void ValidateAndApplySettings(LinqPadConnection connection)
        {
            connection.DatabaseType = DatabaseType.Oracle;

            connection.UserName  =txtUserName.Text;
            connection.Password = txtPassword.Text;
            connection.DbAlias = txtDatabaseAlias.Text;
            connection.SqlCasing = SqlCasing.UpperCase;

            using (OBM.CreateMapper(connection))
            {
                // Try to create a connection
            }
        }
    }
}
