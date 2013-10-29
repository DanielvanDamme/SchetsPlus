using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SchetsEditor
{
    public class Hoofdscherm : Form
    {
        MenuStrip menuStrip;

        public Hoofdscherm()
        {   this.ClientSize = new Size(800, 600);
            menuStrip = new MenuStrip();
            this.Controls.Add(menuStrip);
            this.maakFileMenu();
            this.maakHelpMenu();
            this.Text = "Schets editor";
            this.IsMdiContainer = true;
            this.MainMenuStrip = menuStrip;
        }
        private void maakFileMenu()
        {   ToolStripDropDownItem menu;
            menu = new ToolStripMenuItem("File");
            menu.DropDownItems.Add("Nieuw", null, this.nieuw);
            menu.DropDownItems.Add("Open", null, this.openen);
            menu.DropDownItems.Add("Exit", null, this.afsluiten);
            menuStrip.Items.Add(menu);
        }
        private void maakHelpMenu()
        {   ToolStripDropDownItem menu;
            menu = new ToolStripMenuItem("Help");
            menu.DropDownItems.Add("Over \"Schets\"", null, this.about);
            menuStrip.Items.Add(menu);
        }
        private void about(object o, EventArgs ea)
        {   MessageBox.Show("Schets versie 1.0\n(c) UU Informatica 2010"
                           , "Over \"Schets\""
                           , MessageBoxButtons.OK
                           , MessageBoxIcon.Information
                           );
        }

        private void nieuw(object sender, EventArgs e)
        {
            nieuwWrapper("");
        }
        // 2: new wrapper
        private void nieuwWrapper(string bestandsLocatie)
        {
            SchetsWin s = new SchetsWin(bestandsLocatie);
            s.MdiParent = this;
            s.Show();
            // Abboneer de functie schetsWindowAfsluiten op de FromClosing eventhandler
            s.FormClosing += schetsWindowAfsluiten;
            s.IsBitmapGewijzigd = false;
        }

        private void afsluiten(object sender, EventArgs e)
        {   this.Close();
        }

        // Eventhandler voor het afsluiten van een schets window
        private void schetsWindowAfsluiten(object sender, FormClosingEventArgs e)
        {
            if (((SchetsWin)sender).IsBitmapGewijzigd)
            {
                DialogResult zekerAfsluiten = MessageBox.Show("Er zijn een of meerdere wijzigingen onopgeslagen, \nweet u zeker dat u dit scherm wilt sluiten?", "Schets sluiten", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (zekerAfsluiten == DialogResult.Yes)
                {
                    ((SchetsWin)sender).FormClosing -= schetsWindowAfsluiten;
                    this.ActiveMdiChild.Close();
                }
                else
                    e.Cancel = true;
            }
        }

        // 2: bestand openen methode
        private void openen(object sender, EventArgs e)
        {
            OpenFileDialog bestandOpenen = new OpenFileDialog();
            bestandOpenen.Filter = "JPG file *.jpg|*.jpg|PNG file *.png|*.png|BMP file *.bmp|*.bmp";
            bestandOpenen.Title = "Afbeelding openen";

            if (bestandOpenen.ShowDialog() == DialogResult.OK)
            {
                try
                {   this.nieuwWrapper(bestandOpenen.FileName);   }
                catch (Exception ex)
                {   MessageBox.Show("Fout bij het openen: " + ex.Message, "Fout", MessageBoxButtons.OK, MessageBoxIcon.Error);    }
            }

        }
    }
}
