
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
        public Bitmap bitmap;

        private ObjectControl objectmanager = new ObjectControl();

        public ObjectControl GetManager
        {
            get { return objectmanager; }
        }

        // 2: Property om de wijzigingsstatus op te vragen of door te geven
        public bool IsBitmapGewijzigd
        {   get { return isBitmapGewijzigd; }
            set { isBitmapGewijzigd = value; }
        }
        // 2: Property to get the drawing
        public Bitmap GetBitmap
        {
            get { return bitmap; }
            
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
            {
                if (Path.GetExtension(bestandsLocatie) == ".xml")
                {
                    objectmanager.DeserializeFromXML(bestandsLocatie);
                }
                else
                {
                    bitmap = new Bitmap(bestandsLocatie);
                }
            }
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
        public static void UpdateBitmap(SchetsControl s)
        {
            Bitmap bitmap = s.GetBitmap;
            ObjectControl objectmanager = s.GetManager;
            Bitmap bmp = new Bitmap(bitmap.Width, bitmap.Height);
            Graphics gr = Graphics.FromImage(bmp);

            List<TekenObject> objects = objectmanager.Ophalen;

            Font font = new Font("Tahoma", 40);

            foreach (TekenObject obj in objects)
            {
                Color color = Color.FromName(obj.Kleur);
                SolidBrush brush = new SolidBrush(color);

                switch (obj.Tool)
                {
                    case "tekst":
                        gr.DrawString(obj.Tekst, font, brush, obj.Points[0], StringFormat.GenericDefault);
                        break;
                    case "kader":
                        new RechthoekTool().Teken(gr, obj.Points[0], obj.Points[1], brush);
                        break;
                    case "vlak":
                        new VolRechthoekTool().Teken(gr, obj.Points[0], obj.Points[1], brush);
                        break;
                    case "cirkel":
                        new CirkelTool().Teken(gr, obj.Points[0], obj.Points[1], brush);
                        break;
                    case "rondje":
                        new RondjeTool().Teken(gr, obj.Points[0], obj.Points[1], brush);
                        break;
                    case "lijn":
                        new LijnTool().Teken(gr, obj.Points[0], obj.Points[1], brush);
                        break;
                    case "pen":
                        new PenTool().TekenLijn(gr, obj.Points, brush);
                        break;
                }
            }

            MessageBox.Show("test");
            s.setbitmap = new Bitmap(bitmap.Width, bitmap.Height, gr);

        }
        public void Teken(Graphics gr)
        {
            if (isNieuwScherm)
            {
                gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
                this.isBitmapGewijzigd = false;
                isNieuwScherm = false;
            }

            gr.DrawImage(bitmap, 0, 0);

            this.isBitmapGewijzigd = true;
        }
        public void Schoon()
        {
            Graphics gr = Graphics.FromImage(bitmap);
            //gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
            objectmanager.Reset();
            this.isBitmapGewijzigd = true;
        }
        public void Roteer()
        {
            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
            // 2: Deze actie veroorzaakt een wijziging
            this.isBitmapGewijzigd = true;
        }
        public void Terugdraaien()
        {    
            Graphics gr = Graphics.FromImage(bitmap);
            //gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
            objectmanager.Terugdraaien();
            //DrawFromXML.DrawingFromXML(s, objectmanager.Ophalen);
            // 2: Deze actie veroorzaakt een wijziging
            this.isBitmapGewijzigd = true;
        }
    }
}
