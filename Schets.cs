using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace SchetsEditor
{
    class Schets
    {
        // 2: Wijzigingsstatus van de bitmap bijhouden
        private bool isBitmapGewijzigd = false;
        // 2: Bij initialisatie van een object Schets isNieuwScherm true meegeven
        private bool isNieuwScherm = true;
        private Bitmap bitmap;

        // 2: Property om de wijzigingsstatus op te vragen of door te geven
        public bool IsBitmapGewijzigd
        {   get { return isBitmapGewijzigd; }
            set { isBitmapGewijzigd = value; }
        }
        // 2: Property to get the drawing
        public Bitmap GetBitmap
        {   get { return bitmap; }
        }
        public Graphics BitmapGraphics
        {   get { return Graphics.FromImage(bitmap); }
        }

        // 2: Constructor geschikt gemaakt om een bestand te kunnen openen
        public Schets(string bestandsLocatie)
        {
            bitmap = new Bitmap(1, 1);
            openBestand(bestandsLocatie);
        }

        // 2: Constructor overloading
        public Schets() : this("")
        {
        }
        
        // 2: Bestand openen functie
        private void openBestand(string bestandsLocatie)
        {
            if (bestandsLocatie != "")
                bitmap = new Bitmap(bestandsLocatie);
        }

        public void VeranderAfmeting(Size sz)
        {
            if (sz.Width > bitmap.Size.Width || sz.Height > bitmap.Size.Height)
            {
                Bitmap nieuw = new Bitmap( Math.Max(sz.Width,  bitmap.Size.Width)
                                         , Math.Max(sz.Height, bitmap.Size.Height)
                                         );
                Graphics gr = Graphics.FromImage(nieuw);
                gr.FillRectangle(Brushes.White, 0, 0, sz.Width, sz.Height);
                gr.DrawImage(bitmap, 0, 0);
                bitmap = nieuw;
                // 2: Deze actie veroorzaakt een wijziging
                this.isBitmapGewijzigd = true;
            }
        }
        public void Teken(Graphics gr)
        {
            gr.DrawImage(bitmap, 0, 0);
            // 2: Deze actie veroorzaakt een wijziging, tenzij het scherm net geopend is
            if (isNieuwScherm)
            {
                this.isBitmapGewijzigd = false;
                isNieuwScherm = false;
            }
            else
                this.isBitmapGewijzigd = true;
        }
        public void Schoon()
        {
            Graphics gr = Graphics.FromImage(bitmap);
            gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
            // 2: Deze actie veroorzaakt een wijziging
            this.isBitmapGewijzigd = true;
        }
        public void Roteer()
        {
            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            // 2: Deze actie veroorzaakt een wijziging
            this.isBitmapGewijzigd = true;
        }
    }
}
