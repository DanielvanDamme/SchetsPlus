
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;


namespace SchetsEditor
{
    class Schets
    {
        // 2: Wijzigingsstatus van de bitmap bijhouden
        private bool isBitmapGewijzigd = false;
        // 2: Bij initialisatie van een object Schets isNieuwScherm true meegeven
        private bool isNieuwScherm = true;
        private Bitmap bitmap;
        private ObjectControl objectcontrol = new ObjectControl();
        private static Font font = new Font("Tahoma", 40);

        // De eigenschap font omdat deze buiten deze klasse nodig is
        public static Font Lettertype
        {
            get { return font; }
        }

        public ObjectControl GetController
        {
            get { return objectcontrol; }
        }

        // 2: Property om de wijzigingsstatus op te vragen of door te geven
        public bool IsBitmapGewijzigd
        {
            get { return isBitmapGewijzigd; }
            set { isBitmapGewijzigd = value; }
        }
        // 2: Property to get the drawing
        public Bitmap GetBitmap
        {
            get { return bitmap; }
        }
        public Graphics BitmapGraphics
        {
            get { return Graphics.FromImage(bitmap); }
        }

        // 2: Constructor geschikt gemaakt om een bestand te kunnen openen
        public Schets(string bestandsLocatie)
        {
            bitmap = new Bitmap(1, 1);
            openBestand(bestandsLocatie);
        }

        // 2: Constructor overloading
        public Schets() : this("")
        {}

        // 2: Bestand openen functie
        private void openBestand(string bestandsLocatie)
        {
            if (bestandsLocatie != "")
            {
                if (Path.GetExtension(bestandsLocatie) == ".xml")
                {
                    XmlSerializer deserializer = new XmlSerializer(typeof(List<TekenObject>));
                    TextReader bestandLezer = new StreamReader(bestandsLocatie);
                    objectcontrol.Inladen = (List<TekenObject>)deserializer.Deserialize(bestandLezer);
                    bestandLezer.Close();
                }
                else
                    bitmap = new Bitmap(bestandsLocatie);
            }
        }

        public void VeranderAfmeting(Size sz)
        {
            if (sz.Width > bitmap.Size.Width || sz.Height > bitmap.Size.Height)
            {
                Bitmap nieuw = new Bitmap(Math.Max(sz.Width,  bitmap.Size.Width), Math.Max(sz.Height, bitmap.Size.Height));
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

            if (isNieuwScherm)
            {
                this.isBitmapGewijzigd = false;
                isNieuwScherm = false;
            }
            else
            {
                this.isBitmapGewijzigd = true;
            }
        }

        public void Schoon()
        {
            objectcontrol.Reset();
            Teken(Graphics.FromImage(bitmap), objectcontrol.Ophalen);
            this.isBitmapGewijzigd = true;
        }
        public void Roteer()
        {
            objectcontrol.Roteer(bitmap.Width, bitmap.Height);
            Teken(Graphics.FromImage(bitmap), objectcontrol.Ophalen);
            this.isBitmapGewijzigd = true;
        }
        public void Terugdraaien()
        {
            objectcontrol.Terugdraaien();
            Teken(Graphics.FromImage(bitmap), objectcontrol.Ophalen);
            this.isBitmapGewijzigd = true;
        }

        public static void Teken(Graphics gr, List<TekenObject> objects)
        {
            gr.FillRectangle(Brushes.White, 0, 0, 2560, 1440);
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            foreach (TekenObject obj in objects)
            {
                Color color = Color.FromName(obj.Kleur);
                SolidBrush brush = new SolidBrush(color);

                switch (obj.Tool)
                {
                    case "tekst":
                        SizeF sz = gr.MeasureString(obj.Tekst, font);
                        Bitmap tekstBmp = new Bitmap((int)sz.Width+1, (int)sz.Height+1);
                        Graphics gOff = Graphics.FromImage(tekstBmp);
                        gOff.DrawString(obj.Tekst, font, brush, new Point(0, 0), StringFormat.GenericDefault);

                        if (obj.Hoek == 90)
                            tekstBmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        else if (obj.Hoek == 180)
                            tekstBmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        else if (obj.Hoek == 270)
                            tekstBmp.RotateFlip(RotateFlipType.Rotate270FlipNone);

                        gr.DrawImage(tekstBmp, obj.Points[0]);
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
        }
    }
}
