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
    public partial class AccessSettings : UserControl
    {
        public AccessSettings()
        {
            InitializeComponent();
        }

        private void OnChoose(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
                txtDatabaseFile.Text = openFileDialog.FileName;
        }

        /// <summary>
        /// Initializes the specified connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public void Initialize(LinqPadConnection connection)
        {
            txtDatabaseFile.Text = connection.DatabaseFile;
        }

        /// <summary>
        /// Validates the and apply settings.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public void ValidateAndApplySettings(LinqPadConnection connection)
        {
            connection.DatabaseType = DatabaseType.Access;
            connection.DatabaseFile = txtDatabaseFile.Text;
            connection.SqlCasing = SqlCasing.Mixed;

            using (OBM.CreateMapper(connection))
            {
                // Try to create a connection
            }
        }

    }
}
