
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

        // Property font (deze is buiten de klasse nodig)
        public static Font Lettertype
        {
            get { return font; }
        }

        // Property ObjectControl
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

        // 2: Property om de bitmap op te halen
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
            // Als bestandslocatie niet leeg is het bestand inlezen met de TextReader en de XML data omzetten naar tekenobjecten
            if (bestandsLocatie != "")
            { 
                XmlSerializer deserializer = new XmlSerializer(typeof(List<TekenObject>));
                TextReader bestandLezer = new StreamReader(bestandsLocatie);
                objectcontrol.Inladen = (List<TekenObject>)deserializer.Deserialize(bestandLezer);
                bestandLezer.Close();
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

        // Tekenfunctie die aangeroepen wordt bij Invalidate()
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

        // Wist alles en roept de Teken-functie aan
        public void Schoon()
        {
            objectcontrol.Reset();
            Teken(Graphics.FromImage(bitmap), objectcontrol.Ophalen);
            this.isBitmapGewijzigd = true;
        }

        // Roteert de afbeelding en roept Teken-functie aan
        public void Roteer()
        {
            objectcontrol.Roteer(bitmap.Width, bitmap.Height);
            Teken(Graphics.FromImage(bitmap), objectcontrol.Ophalen);
            this.isBitmapGewijzigd = true;
        }

        // Undo
        public void Terugdraaien()
        {
            objectcontrol.Terugdraaien();
            Teken(Graphics.FromImage(bitmap), objectcontrol.Ophalen);
            this.isBitmapGewijzigd = true;
        }

        // Statische teken overload die alle objecten tekent op basis van het type object, gebruikt hiervoor Tools klasse
        public static void Teken(Graphics gr, List<TekenObject> tekenObjecten)
        {
            // Even alles wit maken
            gr.FillRectangle(Brushes.White, 0, 0, 2560, 1440);
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            foreach (TekenObject tekenObject in tekenObjecten)
            {
                Color color = Color.FromName(tekenObject.Kleur);
                SolidBrush brush = new SolidBrush(color);

                switch (tekenObject.Tool)
                {
                    /* Bij de tekst tool schrijven we de tekst op een bitmap, die roteren we op basis van de meegekregen hoek en vervolgens
                     * voegen we die toe aan de volledige afbeelding */
                    case "tekst":
                        SizeF sz = gr.MeasureString(tekenObject.Tekst, font);
                        Bitmap tekstBmp = new Bitmap((int)sz.Width+1, (int)sz.Height+1);
                        Graphics gOff = Graphics.FromImage(tekstBmp);
                        gOff.DrawString(tekenObject.Tekst, font, brush, new Point(0, 0), StringFormat.GenericDefault);

                        // Op basis van de hoek 'flippen'
                        if (tekenObject.Hoek == 90)
                            tekstBmp.RotateFlip(RotateFlipType.Rotate90FlipNone);
                        else if (tekenObject.Hoek == 180)
                            tekstBmp.RotateFlip(RotateFlipType.Rotate180FlipNone);
                        else if (tekenObject.Hoek == 270)
                            tekstBmp.RotateFlip(RotateFlipType.Rotate270FlipNone);

                        gr.DrawImage(tekstBmp, tekenObject.Points[0]);
                        break;
                    case "kader":
                        new RechthoekTool().Teken(gr, tekenObject.Points[0], tekenObject.Points[1], brush);
                        break;
                    case "vlak":
                        new VolRechthoekTool().Teken(gr, tekenObject.Points[0], tekenObject.Points[1], brush);
                        break;
                    case "cirkel":
                        new CirkelTool().Teken(gr, tekenObject.Points[0], tekenObject.Points[1], brush);
                        break;
                    case "rondje":
                        new RondjeTool().Teken(gr, tekenObject.Points[0], tekenObject.Points[1], brush);
                        break;
                    case "lijn":
                        new LijnTool().Teken(gr, tekenObject.Points[0], tekenObject.Points[1], brush);
                        break;
                    case "pen":
                        new PenTool().TekenLijn(gr, tekenObject.Points, brush);
                        break;
                }
            }
        }
    }
}
