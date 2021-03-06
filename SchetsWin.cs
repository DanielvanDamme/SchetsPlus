﻿using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Resources;
using System.IO;

namespace SchetsEditor
{
    public class SchetsWin : Form
    {
        
        MenuStrip menuStrip;
        SchetsControl schetscontrol;
        ISchetsTool huidigeTool;
        Panel paneel;
        bool vast;
        ResourceManager resourcemanager
            = new ResourceManager("SchetsEditor.Properties.Resources"
                                 , Assembly.GetExecutingAssembly()
                                 );

        // 2: Property om de wijzigingsstatus op te vragen of door te geven
        public bool IsBitmapGewijzigd
        {
            get { return schetscontrol.IsBitmapGewijzigd; }
            set { schetscontrol.IsBitmapGewijzigd = value; }
        }
        private void veranderAfmeting(object o, EventArgs ea)
        {
            schetscontrol.Size = new Size ( this.ClientSize.Width  - 70
                                          , this.ClientSize.Height - 50);
            paneel.Location = new Point(64, this.ClientSize.Height - 30);
        }

        private void klikToolMenu(object obj, EventArgs ea)
        {
            this.huidigeTool = (ISchetsTool)((ToolStripMenuItem)obj).Tag;
        }

        private void klikToolButton(object obj, EventArgs ea)
        {
            this.huidigeTool = (ISchetsTool)((RadioButton)obj).Tag;
        }

        // 2: Nieuwe methode om het bestand op te slaan
        private void bestandOpslaan(object sender, EventArgs ea)
        {
            bool succes = true;

            // Lijst met bestandstypen inclusief SchetsPlus XML
            SaveFileDialog bestandOpslaan = new SaveFileDialog();
            bestandOpslaan.Filter = "SchetsPlus XML *.xml|*.xml|JPG *.jpg|*.jpg|PNG *.png|*.png|BMP *.bmp|*.bmp";
            bestandOpslaan.Title = "Afbeelding opslaan";
            if (bestandOpslaan.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    /* Als het bestand opgeslagen moet worden als XML zetten we onze objecten om naar XML-format, gelukkig kunnen
                     * we hiervoor de XmlSerializer klasse gebruiken die alles voor ons doet */
                    if (Path.GetExtension(bestandOpslaan.FileName) == ".xml")
                    {
                        ObjectControl objectcontrol = schetscontrol.GetController;
                        XmlSerializer serializer = new XmlSerializer(typeof(List<TekenObject>));
                        StreamWriter writer = new StreamWriter(bestandOpslaan.FileName);
                        serializer.Serialize(writer, objectcontrol.Ophalen);
                        writer.Close();
                    }
                    else
                    {
                        Bitmap bmp = schetscontrol.GetBitmap;
                        bmp.Save(bestandOpslaan.FileName);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Fout bij het opslaan: " + e.Message, "Fout", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    succes = false;
                }

                if (succes)
                {
                    schetscontrol.IsBitmapGewijzigd = false;
                    MessageBox.Show("Tekening succesvol opgeslagen in:\n" + bestandOpslaan.FileName, "Bestand opgeslagen", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void afsluiten(object obj, EventArgs ea)
        {
            this.Close();
        }

        public SchetsWin(string bestandsLocatie)
        {
            ISchetsTool[] deTools = { new PenTool()         
                                    , new LijnTool()
                                    , new RechthoekTool()
                                    , new VolRechthoekTool()
                                    , new CirkelTool()
                                    , new RondjeTool()
                                    , new TekstTool()
                                    , new GumTool()
                                    };
            String[] deKleuren = { "Black", "Red", "Green", "Blue"
                                 , "Yellow", "Magenta", "Cyan", "White"
                                 };

            this.ClientSize = new Size(700, 512);
            huidigeTool = deTools[0];


            // 2: Bestandslocatie meegeven aan SchetsControl
            schetscontrol = new SchetsControl(bestandsLocatie);
            schetscontrol.Location = new Point(64, 10);
            schetscontrol.MouseDown += (object o, MouseEventArgs mea) =>
                                       {   vast=true;  
                                           huidigeTool.MuisVast(schetscontrol, mea.Location); 
                                       };
            schetscontrol.MouseMove += (object o, MouseEventArgs mea) =>
                                       {   if (vast)
                                           huidigeTool.MuisDrag(schetscontrol, mea.Location); 
                                       };
            schetscontrol.MouseUp   += (object o, MouseEventArgs mea) =>
                                       {   vast=false; 
                                           huidigeTool.MuisLos (schetscontrol, mea.Location);
                                       };
            schetscontrol.KeyPress +=  (object o, KeyPressEventArgs kpea) => 
                                       {   huidigeTool.Letter  (schetscontrol, kpea.KeyChar); 
                                       };
            this.Controls.Add(schetscontrol);

            menuStrip = new MenuStrip();
            menuStrip.Visible = false;
            this.Icon = new Icon("palet.ico");
            this.Controls.Add(menuStrip);
            this.maakFileMenu();
            this.maakToolMenu(deTools);
            this.maakAktieMenu(deKleuren);
            this.maakToolButtons(deTools);
            this.maakAktieButtons(deKleuren);
            this.Resize += this.veranderAfmeting;
            this.veranderAfmeting(null, null);
            // Teken de objecten nadat een XML bestand is ingeladen
            Schets.Teken(Graphics.FromImage(schetscontrol.GetBitmap), schetscontrol.GetController.Ophalen);
        }

        // 2: overloaded constructor
        public SchetsWin() : this("")
        {
        }

        private void maakFileMenu()
        {
            ToolStripMenuItem menu = new ToolStripMenuItem("File");
            menu.MergeAction = MergeAction.MatchOnly;
            // 2: Menu item voor opslaan
            menu.DropDownItems.Add("Opslaan", null, this.bestandOpslaan);
            menu.DropDownItems.Add("Sluiten", null, this.afsluiten);
            menuStrip.Items.Add(menu);
        }

        private void maakToolMenu(ICollection<ISchetsTool> tools)
        {
            ToolStripMenuItem menu = new ToolStripMenuItem("Tool");
            foreach (ISchetsTool tool in tools)
            {   ToolStripItem item = new ToolStripMenuItem();
                item.Tag = tool;
                item.Text = tool.ToString();
                item.Image = (Image)resourcemanager.GetObject(tool.ToString());
                item.Click += this.klikToolMenu;
                menu.DropDownItems.Add(item);
            }
            menuStrip.Items.Add(menu);
        }

        private void maakAktieMenu(String[] kleuren)
        {   
            ToolStripMenuItem menu = new ToolStripMenuItem("Aktie");
            menu.DropDownItems.Add("Clear", null, schetscontrol.Schoon );
            menu.DropDownItems.Add("Roteer", null, schetscontrol.Roteer );
            ToolStripMenuItem submenu = new ToolStripMenuItem("Kies kleur");
            foreach (string k in kleuren)
                submenu.DropDownItems.Add(k, null, schetscontrol.VeranderKleurViaMenu);
            menu.DropDownItems.Add(submenu);
            menuStrip.Items.Add(menu);
        }

        private void maakToolButtons(ICollection<ISchetsTool> tools)
        {
            int t = 0;
            foreach (ISchetsTool tool in tools)
            {
                RadioButton b = new RadioButton();
                b.Appearance = Appearance.Button;
                b.Size = new Size(45, 62);
                b.Location = new Point(10, 10 + t * 62);
                b.Tag = tool;
                b.Text = tool.ToString();
                b.Image = (Image)resourcemanager.GetObject(tool.ToString());
                b.TextAlign = ContentAlignment.TopCenter;
                b.ImageAlign = ContentAlignment.BottomCenter;
                b.Click += this.klikToolButton;
                this.Controls.Add(b);
                if (t == 0) b.Select();
                t++;
            }
        }

        private void maakAktieButtons(String[] kleuren)
        {   
            paneel = new Panel();
            paneel.Size = new Size(600, 24);
            this.Controls.Add(paneel);
            
            Button b; Label l; ComboBox cbb;
            b = new Button(); 
            b.Text = "Clear";  
            b.Location = new Point(  0, 0); 
            b.Click += schetscontrol.Schoon; 
            paneel.Controls.Add(b);

            b = new Button();
            b.Text = "Rotate";
            b.Location = new Point(80, 0);
            b.Click += schetscontrol.Roteer;
            paneel.Controls.Add(b);

            b = new Button();
            b.Text = "Undo";
            b.Location = new Point(160, 0);
            b.Click += schetscontrol.Terugdraaien;
            paneel.Controls.Add(b);
            
            l = new Label();  
            l.Text = "Penkleur:"; 
            l.Location = new Point(240, 3); 
            l.AutoSize = true;               
            paneel.Controls.Add(l);
            
            cbb = new ComboBox(); cbb.Location = new Point(300, 0); 
            cbb.DropDownStyle = ComboBoxStyle.DropDownList; 
            cbb.SelectedValueChanged += schetscontrol.VeranderKleur;
            foreach (string k in kleuren)
                cbb.Items.Add(k);
            cbb.SelectedIndex = 0;
            paneel.Controls.Add(cbb);
        }
    }
}
