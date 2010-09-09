namespace ObjectMapper2LinqPad
{
    partial class AccessSettings
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
            this.lblDatabaseFile = new System.Windows.Forms.Label();
            this.txtDatabaseFile = new System.Windows.Forms.TextBox();
            this.cmdChooseDatabase = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.Controls.Add(this.lblDatabaseFile, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtDatabaseFile, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.cmdChooseDatabase, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(372, 157);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // lblDatabaseFile
            // 
            this.lblDatabaseFile.AutoSize = true;
            this.lblDatabaseFile.Location = new System.Drawing.Point(3, 0);
            this.lblDatabaseFile.Name = "lblDatabaseFile";
            this.lblDatabaseFile.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lblDatabaseFile.Size = new System.Drawing.Size(72, 17);
            this.lblDatabaseFile.TabIndex = 0;
            this.lblDatabaseFile.Text = "Database File";
            // 
            // txtDatabaseFile
            // 
            this.txtDatabaseFile.Dock = System.Windows.Forms.DockStyle.Top;
            this.txtDatabaseFile.Location = new System.Drawing.Point(123, 3);
            this.txtDatabaseFile.Name = "txtDatabaseFile";
            this.txtDatabaseFile.Size = new System.Drawing.Size(214, 20);
            this.txtDatabaseFile.TabIndex = 1;
            // 
            // cmdChooseDatabase
            // 
            this.cmdChooseDatabase.Location = new System.Drawing.Point(343, 3);
            this.cmdChooseDatabase.Name = "cmdChooseDatabase";
            this.cmdChooseDatabase.Size = new System.Drawing.Size(26, 23);
            this.cmdChooseDatabase.TabIndex = 2;
            this.cmdChooseDatabase.Text = "...";
            this.cmdChooseDatabase.UseVisualStyleBackColor = true;
            this.cmdChooseDatabase.Click += new System.EventHandler(this.OnChoose);
            // 
            // openFileDialog
            // 
            this.openFileDialog.DefaultExt = "*.mdb";
            this.openFileDialog.Filter = "Microsoft Access File|*.mdb";
            // 
            // AccessSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "AccessSettings";
            this.Size = new System.Drawing.Size(372, 157);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label lblDatabaseFile;
        private System.Windows.Forms.TextBox txtDatabaseFile;
        private System.Windows.Forms.Button cmdChooseDatabase;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
    }
}
