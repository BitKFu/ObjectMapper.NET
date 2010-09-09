namespace ObjectMapper2LinqPad
{
    partial class OracleSettings
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.txtDatabaseAlias = new System.Windows.Forms.TextBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.lblDatabaseAlias = new System.Windows.Forms.Label();
            this.lblUserName = new System.Windows.Forms.Label();
            this.lblPassword = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.txtDatabaseAlias, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.txtPassword, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtUserName, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblDatabaseAlias, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblUserName, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblPassword, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(319, 159);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // txtDatabaseAlias
            // 
            this.txtDatabaseAlias.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDatabaseAlias.Location = new System.Drawing.Point(123, 55);
            this.txtDatabaseAlias.Name = "txtDatabaseAlias";
            this.txtDatabaseAlias.Size = new System.Drawing.Size(193, 20);
            this.txtDatabaseAlias.TabIndex = 13;
            // 
            // txtPassword
            // 
            this.txtPassword.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPassword.Location = new System.Drawing.Point(123, 29);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(193, 20);
            this.txtPassword.TabIndex = 12;
            // 
            // txtUserName
            // 
            this.txtUserName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtUserName.Location = new System.Drawing.Point(123, 3);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(193, 20);
            this.txtUserName.TabIndex = 11;
            // 
            // lblDatabaseAlias
            // 
            this.lblDatabaseAlias.AutoSize = true;
            this.lblDatabaseAlias.Location = new System.Drawing.Point(3, 52);
            this.lblDatabaseAlias.Name = "lblDatabaseAlias";
            this.lblDatabaseAlias.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblDatabaseAlias.Size = new System.Drawing.Size(78, 17);
            this.lblDatabaseAlias.TabIndex = 8;
            this.lblDatabaseAlias.Text = "Database Alias";
            // 
            // lblUserName
            // 
            this.lblUserName.AutoSize = true;
            this.lblUserName.Location = new System.Drawing.Point(3, 0);
            this.lblUserName.Name = "lblUserName";
            this.lblUserName.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblUserName.Size = new System.Drawing.Size(60, 17);
            this.lblUserName.TabIndex = 1;
            this.lblUserName.Text = "User Name";
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Location = new System.Drawing.Point(3, 26);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblPassword.Size = new System.Drawing.Size(53, 17);
            this.lblPassword.TabIndex = 3;
            this.lblPassword.Text = "Password";
            // 
            // OracleSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "OracleSettings";
            this.Size = new System.Drawing.Size(319, 159);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblUserName;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label lblDatabaseAlias;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtDatabaseAlias;
    }
}
