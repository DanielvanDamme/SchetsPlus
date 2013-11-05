using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace SchetsEditor
{
    // De klasse ObjectControl beheert alle getekende objecten en kan daarin aanpassingen doen
    public class ObjectControl
    {
        private List<TekenObject> tekenObjecten; // De lijst met getekende objecten, belangrijkste variabele in hele programma ;) 
        private const int GumRandDikte = 8;

        // Constructor, roept reset aan die een nieuwe lijst aanmaakt voor tekenObjecten
        public ObjectControl()
        {
            Reset();
        }

        // Ophalen van alle objecten
        public List<TekenObject> Ophalen
        {
            get { return tekenObjecten; }
        }

        // Inladen van een hele groep objecten, meestal vanuit een SchetsPlus XML
        public List<TekenObject> Inladen
        {
            set { tekenObjecten = value; }
        }

        // Reset tekenObjecten, handig voor het wissen van het scherm
        public void Reset()
        {
            tekenObjecten = new List<TekenObject>();
        }

        // Roteermethode, roteert alle objecten op basis van het middelpunt van de tekening
        public void Roteer(int width, int height)
        {
            foreach (TekenObject tekenObject in tekenObjecten)
            {
                for (int i = 0; i < tekenObject.Points.Count; i++)
                {
                    // Aanpassen aan ons tijdelijke assenstelsel met 0,0 in het midden van de tekening
                    int A = tekenObject.Points[i].X - width / 2;
                    int B = tekenObject.Points[i].Y - height / 2;

                    // Translatie naar nieuwe punt
                    Point punt = new Point(-B, A);

                    // Terugzetten naar assenstelsel met 0,0 in de linkerbovenhoek
                    int C = -B + width / 2;
                    int D = A + height / 2;

                    // Overschrijven van de oude data
                    tekenObject.Points[i] = new Point(C, D);
                }

                /* Om tekst te roteren hebben we naast een nieuw beginpunt ook de hoek nodig om te weten of de tekst horizontaal, verticaal
                 * of zelfs ondersteboven geschreven moet worden. Daarnaast is er een extra bewerking nodig van het nieuwe punt op basis 
                 van de hoogte en breedte van de te schrijven tekst (dit komt door de wijze waarop we tekst roteren bij het tekenen van
                 de afbeelding, we doen dat namelijk door eerst een bitmap te maken met de tekst horizontaal erop en vervolgens deze bitmap
                 te roteren) */
                if (tekenObject.Tool == "tekst")
                {
                    // Was de hoek 270 graden? Dan wordt deze 0 graden, anders tellen we er 90 bij op
                    tekenObject.Hoek = (tekenObject.Hoek == 270) ? 0 : tekenObject.Hoek + 90;

                    // Afmetingen van de geschreven tekst berekenen
                    Graphics g = Graphics.FromImage(new Bitmap(1, 1));
                    SizeF sz = g.MeasureString(tekenObject.Tekst, new Font("Tahoma", 40));

                    int E = tekenObject.Points[0].X;
                    int F = tekenObject.Points[0].Y;

                    // Extra bewerking op basis van hoogte/breedte en hoek
                    if (tekenObject.Hoek == 0 || tekenObject.Hoek == 180)
                        E -= (int)sz.Width;
                    else if (tekenObject.Hoek == 90 || tekenObject.Hoek == 270)
                        E -= (int)sz.Height;

                    // Oude beginpunt overschrijven
                    tekenObject.Points[0] = new Point(E, F);
                }
            }
        }

        // Methode om een object toe te voegen aan de lijst met objecten
        public void Toewijzen(TekenObject tekenObject)
        {
            tekenObjecten.Add(tekenObject);
        }

        // Methode voor de undo-knop, simpelweg delete het laatst toegevoegde object
        public void Terugdraaien()
        {
            if (tekenObjecten.Count > 0)
                tekenObjecten.RemoveAt(tekenObjecten.Count - 1);
        }

        /* Methode voor het nieuwe gummen. Checkt voor alle objecten vanaf 'boven' naar 'onder' of er raak is geklikt en verwijder het 
         * eerste object dat aan die voorwaarde voldoet */
        public void VerwijderBovensteObjectOpPunt(Point p)
        {
            for (int i = (tekenObjecten.Count - 1); i >= 0; i--)
            {
                if (isRaak(tekenObjecten[i], p))
                {
                    tekenObjecten.RemoveAt(i);
                    break;
                }
            }
        }

        // isRaak roept de juiste methode aan voor het betreffende TekenObject om te controleren of er op is geklikt
        private static bool isRaak(TekenObject obj, Point p)
        {
            switch (obj.Tool)
            {
                case "tekst":
                    return isOpTekstGeklikt(obj, p);
                case "kader":
                    return isOpKaderGeklikt(obj, p);
                case "vlak":
                    return isBinnenVierkant(obj, p, 0);
                case "cirkel":
                    return isEllipsRaakGeklikt(obj, p, false);
                case "rondje":
                    return isEllipsRaakGeklikt(obj, p, true);
                case "lijn":
                    return isOpLijnGeklikt(obj, p);
                case "pen":
                    return isOpPenGeklikt(obj, p);
            }
            return false;
        }

        // Checkt of punt p op de tekst ligt
        private static bool isOpTekstGeklikt(TekenObject obj, Point p)
        {
            Graphics g = Graphics.FromImage(new Bitmap(1,1));

            SizeF sz = g.MeasureString(obj.Tekst, Schets.Lettertype);
            Point begin = obj.Points[0];

            if (p.X > begin.X && p.X < begin.X + sz.Width && p.Y > begin.Y && p.Y < begin.Y + sz.Height)
                return true;

            return false;
        }

        // Check of punt p in de buurt ligt van alle streepjes die behoren tot object van het type Pen
        private static bool isOpPenGeklikt(TekenObject obj, Point p)
        {
            for (int i = 1; i < obj.Points.Count; i++)
            {
                if (afstandVanPuntTotLijn(obj.Points[i - 1], obj.Points[i], p) < GumRandDikte)
                    return true;
            }
            return false;
        }

        // Deze methode bepaalt of het punt p binnen de geaccepteerde marge t.o.v. het lijnstuk ligt
        private static bool isOpLijnGeklikt(TekenObject obj, Point p)
        {
            return (afstandVanPuntTotLijn(obj.Points[0], obj.Points[1], p) < GumRandDikte);
        }

        // Bepaalt of het punt binnen het object ligt en geeft de mogelijkheid om de grootte van de 'bounding box' aan te passen
        private static bool isBinnenVierkant(TekenObject obj, Point p, int aanpassing)
        {
            Point begin = new Point(Math.Min(obj.Points[0].X, obj.Points[1].X), Math.Min(obj.Points[0].Y, obj.Points[1].Y));
            Point eind = new Point(Math.Max(obj.Points[0].X, obj.Points[1].X), Math.Max(obj.Points[0].Y, obj.Points[1].Y));
            return (p.X > begin.X + aanpassing && p.X < eind.X - aanpassing &&
                    p.Y > begin.Y + aanpassing && p.Y < eind.Y - aanpassing);
        }

        /* Laat weten of er op de rand van de rechthoek geklikt is, het kader wordt aan weerskanten vergroot met de helft van de 
         * dikte van de gumrand */
        private static bool isOpKaderGeklikt(TekenObject obj, Point p)
        {
            return (!isBinnenVierkant(obj, p, GumRandDikte) && isBinnenVierkant(obj, p, -GumRandDikte));
        }

        // Bepaalt of punt p binnen of op de lijn van de cirkel ligt afhankelijk van de waarde van de boolean isGevuldeCirkel
        private static bool isEllipsRaakGeklikt(TekenObject obj, Point p, bool isGevuldeEllips)
        {
            Point begin = new Point(Math.Min(obj.Points[0].X, obj.Points[1].X), Math.Min(obj.Points[0].Y, obj.Points[1].Y));
            Point eind = new Point(Math.Max(obj.Points[0].X, obj.Points[1].X), Math.Max(obj.Points[0].Y, obj.Points[1].Y));
            double radiusx = Math.Abs((double)eind.X - (double)begin.X) / 2.0;
            double radiusy = Math.Abs((double)eind.Y - (double)begin.Y) / 2.0;
            Point middelpunt = new Point(begin.X + (int)radiusx, begin.Y + (int)radiusy);

            if (isGevuldeEllips)
                return isPuntBinnenCirkel(p, radiusx, radiusy, middelpunt);
            else
                return isPuntOpRand(p, radiusx, radiusy, middelpunt);
        }

        /* Laat weten of er op de rand van de ellips geklikt is, de ellips wordt aan weerskanten vergroot met de helft van de dikte van de gumrand
         * Maakt gebruik van de stelling van pythagoras om de afstand te berekenen t.o.v. het middelpunt */
        private static bool isPuntOpRand(Point p, double radiusx, double radiusy, Point middelpunt)
        {
            return (Math.Sqrt(Math.Pow((p.X - middelpunt.X) / (radiusx - GumRandDikte), 2) + Math.Pow((p.Y - middelpunt.Y) / (radiusy - GumRandDikte), 2)) >= 1
                && isPuntBinnenCirkel(p, radiusx + GumRandDikte, radiusy + GumRandDikte, middelpunt));
        }

        // Deze functie bepaalt of het punt binnen de cirkel ligt
        private static bool isPuntBinnenCirkel(Point p, double radiusx, double radiusy, Point middelpunt)
        {
            return (Math.Sqrt(Math.Pow((p.X - middelpunt.X) / radiusx, 2) + Math.Pow((p.Y - middelpunt.Y) / radiusy, 2)) <= 1);
        }

        /* Deze methode is afgeleid van http://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line
         * en berekent de kortste afstand vanaf punt p tot aan de lijn die loop vanaf het punt begin tot het punt eind */
        private static double afstandVanPuntTotLijn(Point begin, Point eind, Point p)
        {
            double xx, yy;

            double dx = eind.X - begin.X;
            double dy = eind.Y - begin.Y;

            double res = ((p.X - begin.X) * dx + (p.Y - begin.Y) * dy) / (dx * dx + dy * dy);

            // Bepaal welk punt op de lijn(begin, eind) het dichtsbij het geklikte punt p ligt
            // Als het resultaat kleiner is dan 0, dan ligt het beginpunt van de lijn het dichtste bij punt p
            if (res < 0)
            {
                xx = begin.X;
                yy = begin.Y;
            }
            // Als het resultaat groter is dan 1, dan ligt het eindpunt van de lijn het dichtste bij punt p
            else if (res > 1)
            {
                xx = eind.X;
                yy = eind.Y;
            }
            // Anders is het een punt op de lijn het dichtste bij
            else
            {
                xx = begin.X + res * dx;
                yy = begin.Y + res * dy;
            }

            return Math.Sqrt(Math.Pow(p.X - xx, 2) + Math.Pow(p.Y - yy, 2));
        }
    }
}
