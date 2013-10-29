using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SchetsEditor
{
    public class SchetsControl : UserControl
    {
        private Schets schets;
        private Color penkleur;

        // 2: Property om de tekening op te vragen (om het op te slaan)
        public Bitmap GetBitmap
        {
            get { return schets.GetBitmap; }
        }
        // 2: Property om de wijzigingsstatus op te vragen of door te geven
        public bool IsBitmapGewijzigd
        {
            get { return schets.IsBitmapGewijzigd; }
            set { schets.IsBitmapGewijzigd = value; }
        }
        public Color PenKleur 
        {   get { return penkleur; } 
        }
        // 2: Constructor geschikt gemaakt om een bestand te kunnen openen
        public SchetsControl(string bestandsLocatie)
        {   this.BorderStyle = BorderStyle.Fixed3D;
            this.schets = new Schets(bestandsLocatie);
            this.Paint += this.teken;
            this.Resize += this.veranderAfmeting;
            this.veranderAfmeting(null, null);
        }
        // 2: Constructor overloading
        public SchetsControl() : this("")
        { }
        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }
        private void teken(object o, PaintEventArgs pea)
        {   schets.Teken(pea.Graphics);
        }
        private void veranderAfmeting(object o, EventArgs ea)
        {   schets.VeranderAfmeting(this.ClientSize);
            this.Invalidate();
        }
        public Graphics MaakBitmapGraphics()
        {   Graphics g = schets.BitmapGraphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            return g;
        }
        public void Schoon(object o, EventArgs ea)
        {   schets.Schoon();
            this.Invalidate();
        }
        public void Roteer(object o, EventArgs ea)
        {   schets.Roteer();
            this.veranderAfmeting(o, ea);
        }
        public void VeranderKleur(object obj, EventArgs ea)
        {   string kleurNaam = ((ComboBox)obj).Text;
            penkleur = Color.FromName(kleurNaam);
        }
        public void VeranderKleurViaMenu(object obj, EventArgs ea)
        {   string kleurNaam = ((ToolStripMenuItem)obj).Text;
            penkleur = Color.FromName(kleurNaam);
        }
    }
}
