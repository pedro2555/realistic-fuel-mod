namespace DashboardPositioner
{
    partial class mainForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblRes = new System.Windows.Forms.Label();
            this.btnExit = new System.Windows.Forms.Button();
            this.pnlDashboard = new System.Windows.Forms.Panel();
            this.lblDashboard = new System.Windows.Forms.Label();
            this.btnCopy = new System.Windows.Forms.Button();
            this.lblHelp = new System.Windows.Forms.Label();
            this.nupWidth = new System.Windows.Forms.NumericUpDown();
            this.btnSave = new System.Windows.Forms.Button();
            this.lblWidthText = new System.Windows.Forms.Label();
            this.lblHelp1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.nupWidth)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblRes
            // 
            this.lblRes.AutoSize = true;
            this.lblRes.BackColor = System.Drawing.Color.Transparent;
            this.lblRes.ForeColor = System.Drawing.Color.Lime;
            this.lblRes.Location = new System.Drawing.Point(161, 12);
            this.lblRes.Name = "lblRes";
            this.lblRes.Size = new System.Drawing.Size(98, 13);
            this.lblRes.TabIndex = 0;
            this.lblRes.Text = "Current resolution : ";
            // 
            // btnExit
            // 
            this.btnExit.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
            this.btnExit.FlatAppearance.BorderSize = 0;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.ForeColor = System.Drawing.Color.Lime;
            this.btnExit.Location = new System.Drawing.Point(219, 111);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(44, 23);
            this.btnExit.TabIndex = 1;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // pnlDashboard
            // 
            this.pnlDashboard.BackColor = System.Drawing.Color.Black;
            this.pnlDashboard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlDashboard.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pnlDashboard.ForeColor = System.Drawing.Color.DarkGreen;
            this.pnlDashboard.Location = new System.Drawing.Point(368, 300);
            this.pnlDashboard.Name = "pnlDashboard";
            this.pnlDashboard.Size = new System.Drawing.Size(131, 10);
            this.pnlDashboard.TabIndex = 2;
            this.pnlDashboard.LocationChanged += new System.EventHandler(this.pnlDashboard_LocationChanged);
            this.pnlDashboard.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlDashboard_Paint);
            this.pnlDashboard.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlDashboard_MouseDown);
            this.pnlDashboard.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlDashboard_MouseMove);
            this.pnlDashboard.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pnlDashboard_MouseUp);
            // 
            // lblDashboard
            // 
            this.lblDashboard.AutoSize = true;
            this.lblDashboard.BackColor = System.Drawing.Color.Transparent;
            this.lblDashboard.ForeColor = System.Drawing.Color.Lime;
            this.lblDashboard.Location = new System.Drawing.Point(36, 12);
            this.lblDashboard.Name = "lblDashboard";
            this.lblDashboard.Size = new System.Drawing.Size(105, 13);
            this.lblDashboard.TabIndex = 0;
            this.lblDashboard.Text = "Dashboard location :";
            // 
            // btnCopy
            // 
            this.btnCopy.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
            this.btnCopy.FlatAppearance.BorderSize = 0;
            this.btnCopy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCopy.ForeColor = System.Drawing.Color.Lime;
            this.btnCopy.Location = new System.Drawing.Point(31, 111);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(108, 23);
            this.btnCopy.TabIndex = 1;
            this.btnCopy.Text = "Copy to clipboard";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // lblHelp
            // 
            this.lblHelp.AutoSize = true;
            this.lblHelp.BackColor = System.Drawing.Color.Transparent;
            this.lblHelp.ForeColor = System.Drawing.Color.Gold;
            this.lblHelp.Location = new System.Drawing.Point(50, 150);
            this.lblHelp.Name = "lblHelp";
            this.lblHelp.Size = new System.Drawing.Size(195, 13);
            this.lblHelp.TabIndex = 0;
            this.lblHelp.Text = "Move this around to the desired position";
            // 
            // nupWidth
            // 
            this.nupWidth.BackColor = System.Drawing.Color.Gray;
            this.nupWidth.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.nupWidth.DecimalPlaces = 3;
            this.nupWidth.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.nupWidth.ForeColor = System.Drawing.Color.Lime;
            this.nupWidth.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.nupWidth.Location = new System.Drawing.Point(140, 76);
            this.nupWidth.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nupWidth.Name = "nupWidth";
            this.nupWidth.Size = new System.Drawing.Size(65, 18);
            this.nupWidth.TabIndex = 3;
            this.nupWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nupWidth.Value = new decimal(new int[] {
            11,
            0,
            0,
            131072});
            this.nupWidth.ValueChanged += new System.EventHandler(this.nupWidth_ValueChanged);
            // 
            // btnSave
            // 
            this.btnSave.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.ForeColor = System.Drawing.Color.Lime;
            this.btnSave.Location = new System.Drawing.Point(145, 111);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(68, 23);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Save to .ini";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // lblWidthText
            // 
            this.lblWidthText.AutoSize = true;
            this.lblWidthText.BackColor = System.Drawing.Color.Transparent;
            this.lblWidthText.ForeColor = System.Drawing.Color.Lime;
            this.lblWidthText.Location = new System.Drawing.Point(90, 77);
            this.lblWidthText.Name = "lblWidthText";
            this.lblWidthText.Size = new System.Drawing.Size(44, 13);
            this.lblWidthText.TabIndex = 0;
            this.lblWidthText.Text = "Width : ";
            // 
            // lblHelp1
            // 
            this.lblHelp1.AutoSize = true;
            this.lblHelp1.BackColor = System.Drawing.Color.Transparent;
            this.lblHelp1.ForeColor = System.Drawing.Color.Gold;
            this.lblHelp1.Location = new System.Drawing.Point(78, 60);
            this.lblHelp1.Name = "lblHelp1";
            this.lblHelp1.Size = new System.Drawing.Size(139, 13);
            this.lblHelp1.TabIndex = 0;
            this.lblHelp1.Text = "Adjust the gauge width here";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnSave);
            this.panel1.Controls.Add(this.lblHelp);
            this.panel1.Controls.Add(this.nupWidth);
            this.panel1.Controls.Add(this.btnExit);
            this.panel1.Controls.Add(this.lblWidthText);
            this.panel1.Controls.Add(this.btnCopy);
            this.panel1.Controls.Add(this.lblHelp1);
            this.panel1.Controls.Add(this.lblDashboard);
            this.panel1.Controls.Add(this.lblRes);
            this.panel1.Location = new System.Drawing.Point(286, 124);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(294, 170);
            this.panel1.TabIndex = 4;
            // 
            // mainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(866, 434);
            this.Controls.Add(this.pnlDashboard);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "mainForm";
            this.Opacity = 0.95D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Dashboard Position for Realistic Fuel Mod";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.mainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nupWidth)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblRes;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Panel pnlDashboard;
        private System.Windows.Forms.Label lblDashboard;
        private System.Windows.Forms.Button btnCopy;
        private System.Windows.Forms.Label lblHelp;
        private System.Windows.Forms.NumericUpDown nupWidth;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Label lblWidthText;
        private System.Windows.Forms.Label lblHelp1;
        private System.Windows.Forms.Panel panel1;
    }
}

