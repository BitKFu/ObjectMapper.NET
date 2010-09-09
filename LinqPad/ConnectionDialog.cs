using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using AdFactum.Data.Util;
using LINQPad.Extensibility.DataContext;

namespace ObjectMapper2LinqPad
{
    public partial class ConnectionDialog : Form
    {
        public class DatabaseTypeInfo
        {
            public DatabaseType DatabaseType { get; set; }

            /// <summary> Returns a <see cref="System.String"/> that represents this instance. </summary>
            public override string ToString()
            {
                return DatabaseType.ToString();
            }
        }

        public LinqPadConnection Connection { get; private set; }

        private SqlServerSettings sqlServerSettings;
        private OracleSettings oracleSettings;
        private AccessSettings accessSettings;
        private PostgresSettings postgresSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionDialog"/> class.
        /// </summary>
        public ConnectionDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionDialog"/> class.
        /// </summary>
        /// <param name="cxInfo">The cx info.</param>
        public ConnectionDialog(IConnectionInfo cxInfo)
        {
            InitializeComponent();
            Connection = new LinqPadConnection(cxInfo);

            sqlServerSettings = new SqlServerSettings();
            oracleSettings = new OracleSettings();
            accessSettings = new AccessSettings();
            postgresSettings = new PostgresSettings();
        }

        /// <summary>
        /// Called when [load].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnLoad(object sender, EventArgs e)
        {
            // Add Valid Database Types
            cboDatabaseType.Items.Clear();
            cboDatabaseType.Items.Add(new DatabaseTypeInfo {DatabaseType = DatabaseType.Access});
            cboDatabaseType.Items.Add(new DatabaseTypeInfo {DatabaseType = DatabaseType.SqlServer});
            cboDatabaseType.Items.Add(new DatabaseTypeInfo {DatabaseType = DatabaseType.SqlServer2000});
            cboDatabaseType.Items.Add(new DatabaseTypeInfo {DatabaseType = DatabaseType.Postgres});
            cboDatabaseType.Items.Add(new DatabaseTypeInfo {DatabaseType = DatabaseType.Oracle});

            if (Connection != null)
            {
                foreach (DatabaseTypeInfo item in cboDatabaseType.Items)
                    if (item.DatabaseType == Connection.DatabaseType)
                    {
                        cboDatabaseType.SelectedItem = item;
                        break;
                    }
                txtLinqProvider.Text = Connection.CxInfo.CustomTypeInfo.CustomAssemblyPath;
            }
        }

        /// <summary>
        /// Called when [selection changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnSelectionChanged(object sender, EventArgs e)
        {
            var selectedItem = cboDatabaseType.SelectedItem as DatabaseTypeInfo;
            if (selectedItem == null)
                return;

            switch (selectedItem.DatabaseType)
            {
                case DatabaseType.Access:
                    panel.Controls.Clear();
                    panel.Controls.Add(accessSettings);
                    accessSettings.Initialize(Connection);
                    break;
                case DatabaseType.SqlServer:
                case DatabaseType.SqlServer2000:
                    panel.Controls.Clear();
                    panel.Controls.Add(sqlServerSettings);
                    sqlServerSettings.Initialize(Connection);
                    break;
                case DatabaseType.Oracle:
                    panel.Controls.Clear();
                    panel.Controls.Add(oracleSettings);
                    oracleSettings.Initialize(Connection);
                    break;
                case DatabaseType.Postgres:
                    panel.Controls.Clear();
                    panel.Controls.Add(postgresSettings);
                    postgresSettings.Initialize(Connection);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Called when [cancel].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnCancel(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Called when [ok].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnOk(object sender, EventArgs e)
        {
            var selectedItem = cboDatabaseType.SelectedItem as DatabaseTypeInfo;
            if (selectedItem == null)
                return;

            try
            {
                switch (selectedItem.DatabaseType)
                {
                    case DatabaseType.Access:
                        accessSettings.ValidateAndApplySettings(Connection);
                        break;
                    case DatabaseType.SqlServer:
                    case DatabaseType.SqlServer2000:
                        sqlServerSettings.ValidateAndApplySettings(Connection);
                        break;
                    case DatabaseType.Oracle:
                        oracleSettings.ValidateAndApplySettings(Connection);
                        break;
                    case DatabaseType.Postgres:
                        postgresSettings.ValidateAndApplySettings(Connection);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                ValidateAndApplySettings();

                DialogResult = DialogResult.OK;
                Close();
            }
            catch(Exception exc)
            {
                MessageBox.Show(this, exc.Message, "Connection Failure", MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
                return;
            }
        }

        /// <summary>
        /// Validates the and apply settings.
        /// </summary>
        private void ValidateAndApplySettings()
        {
            Connection.CxInfo.CustomTypeInfo.CustomAssemblyPath = txtLinqProvider.Text;
            if (string.IsNullOrEmpty(txtLinqProvider.Text))
                throw new Exception("Valid LinqProvider DLL must be specified.");

            if (!File.Exists(Connection.CxInfo.CustomTypeInfo.CustomAssemblyPath))
                throw new FileNotFoundException("LinqProvider could not be found", Connection.CxInfo.CustomTypeInfo.CustomAssemblyPath);

            List<string> linqProvider = Connection.CxInfo.CustomTypeInfo.GetCustomTypesInAssembly().Where(name => name.EndsWith(".LinqProvider")).ToList();
            if (linqProvider.Count == 0)
                throw new Exception("No class with the name LinqProvider could be found");

            if (linqProvider.Count > 1)
                throw new Exception("More than 1 LinqProvider found.");

            Connection.CxInfo.CustomTypeInfo.CustomTypeName = linqProvider.First();
        }

        /// <summary>
        /// Called when [choose linq provider].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void OnChooseLinqProvider(object sender, EventArgs e)
        {
            if (linqProvider.ShowDialog(this) == DialogResult.OK)
                txtLinqProvider.Text = linqProvider.FileName;
        }
    }
}
