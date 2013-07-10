using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;

namespace DashboardPositioner
{
    public partial class mainForm : Form
    {
        private Point _offSet;
        float x, y, w;

        public mainForm()
        {
            InitializeComponent();
            this.Icon = new System.Drawing.Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("DashboardPositioner.GTAIV.ico"));
            this.lblRes.Text = String.Format("Current resolution : {0}x{1}", this.Width, this.Height);
            _offSet = Point.Empty;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            this.lblRes.Text = String.Format("Current resolution :\n{0}x{1}", this.Width, this.Height);
            this.pnlDashboard_LocationChanged(this, e);
            
            w = .11f;
            this.pnlDashboard.Width = (int)((w * 100) * this.Width) / 100;
            this.panel1.Location = new Point((this.Width / 2) - (this.panel1.Width / 2), (this.Height / 2) - (this.panel1.Height / 2));
            this.pnlDashboard.Location = new Point((this.Width / 2) - (this.pnlDashboard.Width / 2), (this.Height / 2) + (this.panel1.Height / 2));
        }

        private void pnlDashboard_LocationChanged(object sender, EventArgs e)
        {
            x = (pnlDashboard.Location.X * 100.0f) / this.Width;
            y = (pnlDashboard.Location.Y * 100.0f) / this.Height;
            this.lblDashboard.Text = String.Format("Dashboard location :\nx={0}\ny={1}", x / 100.0f, y / 100.0f);
        }

        private void pnlDashboard_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                _offSet = new Point(e.X, e.Y);
            this.lblHelp.Visible = false;
        }

        private void pnlDashboard_MouseUp(object sender, MouseEventArgs e)
        {
            _offSet = Point.Empty;
            pnlDashboard.Invalidate();
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
            x = (pnlDashboard.Location.X * 100.0f) / this.Width;
            y = (pnlDashboard.Location.Y * 100.0f) / this.Height;

            Clipboard.SetText(String.Format("X={0}\nY={1}\nW={2}", x / 100.0f, y / 100.0f, w).Replace(',', '.'));

            MessageBox.Show(String.Format("X={0}\nY={1}\nW={2}", x / 100.0f, y / 100.0f, w).Replace(',', '.') + "\n\nCopied to clipboard. Press CTRL + V, to paste it.", "Position copied");
        }

        private void pnlDashboard_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                pnlDashboard.CreateGraphics().FillRectangle(
                    new SolidBrush(this.pnlDashboard.ForeColor), 
                    new Rectangle(2, 2, pnlDashboard.Width - 6, pnlDashboard.Height - 6));
            }
            catch (Exception crap)
            {
                
            }
        }

        private void nupWidth_ValueChanged(object sender, EventArgs e)
        {
            lblHelp1.Visible = false;
            w = (float)nupWidth.Value;
            this.pnlDashboard.Width = (int)((w * 100) * this.Width) / 100;
            this.pnlDashboard.Invalidate();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\FuelScript.ini";
            if (File.Exists(path))
            {
                SaveToFile(path);
            }
            else
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "FuelScript.ini | FuelScript.ini";
                dialog.CheckFileExists = true;
                dialog.InitialDirectory = "C:\\";
                dialog.Title = "Please indicate where your FuelScript.ini file is.";
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    SaveToFile(dialog.FileName);
            }
        }


        private void SaveToFile(string filePath)
        {
            StreamReader sr;
            StreamWriter sw;
            try
            {
                sr = new StreamReader(filePath);
                List<string> lines = new List<string>();
                try
                {
                    string line = "";
                    do
                    {
                        line = sr.ReadLine();
                        if (line.ToUpper().StartsWith("X="))
                            lines.Add("X=" + x.ToString().Replace(',', '.'));
                        else if (line.ToUpper().StartsWith("Y="))
                            lines.Add("Y=" + y.ToString().Replace(',', '.'));
                        else if (line.ToUpper().StartsWith("W="))
                            lines.Add("W=" + w.ToString().Replace(',', '.'));
                        else
                            lines.Add(line);
                    } while (line != null);
                }
                catch (Exception)
                {

                }
                finally
                {
                    sr.Close();
                }
                sw = new StreamWriter(filePath);
                foreach (string l in lines)
                    sw.WriteLine(l);
                sw.Close();

                MessageBox.Show("Settings file updated.");
            }
            catch (Exception crap)
            {
                MessageBox.Show("Failed to update file.\n" + crap.Message);
            }
        }
    }
}
