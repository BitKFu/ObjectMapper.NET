namespace ObjectMapper2LinqPad
{
    partial class PostgresSettings
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtDatabase = new System.Windows.Forms.TextBox();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.txtServer = new System.Windows.Forms.TextBox();
            this.lblUserName = new System.Windows.Forms.Label();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.lblServerName = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.lblSqlCasing = new System.Windows.Forms.Label();
            this.chkLowerCasing = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtDatabase
            // 
            this.txtDatabase.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDatabase.Location = new System.Drawing.Point(123, 29);
            this.txtDatabase.Name = "txtDatabase";
            this.txtDatabase.Size = new System.Drawing.Size(190, 20);
            this.txtDatabase.TabIndex = 1;
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(3, 26);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblDatabase.Size = new System.Drawing.Size(53, 17);
            this.lblDatabase.TabIndex = 7;
            this.lblDatabase.Text = "Database";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.txtDatabase, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblDatabase, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtServer, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblUserName, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.txtUserName, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.txtPassword, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.lblServerName, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblPassword, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.lblSqlCasing, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.chkLowerCasing, 1, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(316, 175);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // txtServer
            // 
            this.txtServer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtServer.Location = new System.Drawing.Point(123, 3);
            this.txtServer.Name = "txtServer";
            this.txtServer.Size = new System.Drawing.Size(190, 20);
            this.txtServer.TabIndex = 0;
            // 
            // lblUserName
            // 
            this.lblUserName.AutoSize = true;
            this.lblUserName.Location = new System.Drawing.Point(3, 95);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblUserName.Size = new System.Drawing.Size(60, 17);
            this.lblUserName.TabIndex = 0;
            this.lblUserName.Text = "User Name";
            // 
            // txtUserName
            // 
            this.txtUserName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtUserName.Location = new System.Drawing.Point(123, 98);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(190, 20);
            this.txtUserName.TabIndex = 3;
            // 
            // txtPassword
            // 
            this.txtPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPassword.Location = new System.Drawing.Point(123, 124);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(190, 20);
            this.txtPassword.TabIndex = 4;
            // 
            // lblServerName
            // 
            this.lblServerName.AutoSize = true;
            this.lblServerName.Location = new System.Drawing.Point(3, 0);
            this.lblServerName.Name = "lblServerName";
            this.lblServerName.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblServerName.Size = new System.Drawing.Size(38, 17);
            this.lblServerName.TabIndex = 5;
            this.lblServerName.Text = "Server";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(3, 121);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblPassword.Size = new System.Drawing.Size(53, 17);
            this.lblPassword.TabIndex = 2;
            this.lblPassword.Text = "Password";
            // 
            // lblSqlCasing
            // 
            this.lblSqlCasing.AutoSize = true;
            this.lblSqlCasing.Location = new System.Drawing.Point(3, 52);
            this.lblSqlCasing.Name = "lblSqlCasing";
            this.lblSqlCasing.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblSqlCasing.Size = new System.Drawing.Size(57, 17);
            this.lblSqlCasing.TabIndex = 8;
            this.lblSqlCasing.Text = "Sql Casing";
            // 
            // chkLowerCasing
            // 
            this.chkLowerCasing.AutoSize = true;
            this.chkLowerCasing.Location = new System.Drawing.Point(123, 55);
            this.chkLowerCasing.Name = "chkLowerCasing";
            this.chkLowerCasing.Size = new System.Drawing.Size(120, 17);
            this.chkLowerCasing.TabIndex = 9;
            this.chkLowerCasing.Text = "Force Lower Casing";
            this.chkLowerCasing.UseVisualStyleBackColor = true;
            // 
            // PostgresSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "PostgresSettings";
            this.Size = new System.Drawing.Size(316, 175);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox txtDatabase;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox txtServer;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label lblServerName;
        private System.Windows.Forms.Label lblSqlCasing;
        private System.Windows.Forms.CheckBox chkLowerCasing;
    }
}
