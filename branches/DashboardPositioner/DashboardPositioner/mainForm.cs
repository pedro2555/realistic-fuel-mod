using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DashboardPositioner
{
    public partial class mainForm : Form
    {
        private Point _offSet;

        public mainForm()
        {
            InitializeComponent();
            this.Icon = new System.Drawing.Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("DashboardPositioner.GTAIV.ico"));
            this.lblRes.Text = String.Format("Current resolution : {0}x{1}", this.Width, this.Height);
            _offSet = Point.Empty;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            this.lblRes.Text = String.Format("Current resolution : {0}x{1}", this.Width, this.Height);
            this.pnlDashboard_LocationChanged(this, e);
        }

        private void pnlDashboard_LocationChanged(object sender, EventArgs e)
        {
            float x, y;
            x = (pnlDashboard.Location.X * 100) / this.Width;
            y = (pnlDashboard.Location.Y * 100) / this.Height;
            this.lblDashboard.Text = String.Format("Dashboard location : x={0} y={1}", x / 100, y / 100);
        }

        private void pnlDashboard_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _offSet = new Point(e.X, e.Y);
        }

        private void pnlDashboard_MouseUp(object sender, MouseEventArgs e)
        {
            _offSet = Point.Empty;
        }

        private void pnlDashboard_MouseMove(object sender, MouseEventArgs e)
        {
            if (_offSet != Point.Empty)
            {
                Point newlocation = this.pnlDashboard.Location;
                newlocation.X += e.X - _offSet.X;
                newlocation.Y += e.Y - _offSet.Y;
                this.pnlDashboard.Location = newlocation;
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            float x, y;
            x = (pnlDashboard.Location.X * 100) / this.Width;
            y = (pnlDashboard.Location.Y * 100) / this.Height;

            Clipboard.SetText(String.Format("X={0}\nY={1}", x / 100, y / 100).Replace(',', '.'));

            MessageBox.Show(String.Format("X={0}\nY={1}", x / 100, y / 100).Replace(',', '.') + "\nCopied to clipboard. Press CTRL + V, to paste it.", "Position copied");
        }

    }
}
