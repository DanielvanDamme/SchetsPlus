using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace SchetsEditor
{
    public class ObjectControl
    {
        private List<TekenObject> tekenObjecten = new List<TekenObject>();
        private const int GumRandDikte = 8;

        public List<TekenObject> Ophalen
        {
            get
            {
                return tekenObjecten;
            }
        }

        public ObjectControl()
        {
            Reset();
        }

        public void Reset()
        {
            tekenObjecten = new List<TekenObject>();
        }

        public void Roteer(int width, int height)
        {
            foreach (TekenObject tekenObject in tekenObjecten)
            {
                for (int i = 0; i < tekenObject.Points.Count; i++)
                {
                    int A = tekenObject.Points[i].X - width / 2;
                    int B = tekenObject.Points[i].Y - height / 2;

                    Point punt = new Point(-B, A);

                    int C = -B + width / 2;
                    int D = A + height / 2;

                    tekenObject.Points[i] = new Point(C, D);
                }

                if (tekenObject.Tool == "tekst")
                {
                    tekenObject.Hoek = (tekenObject.Hoek == 270) ? 0 : tekenObject.Hoek + 90;
                }
            }
        }

        public void SerializeToXML(string bestandsnaam)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<TekenObject>));
            StreamWriter writer = new StreamWriter(bestandsnaam);
            serializer.Serialize(writer, this.tekenObjecten);
            writer.Close();
        }

        public void DeserializeFromXML(string bestandsnaam)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(List<TekenObject>));
            TextReader textReader = new StreamReader(bestandsnaam);
            this.tekenObjecten = (List<TekenObject>)deserializer.Deserialize(textReader);
            textReader.Close();
        }

        public void Toewijzen(TekenObject tekenObject)
        {
            tekenObjecten.Add(tekenObject);
        }

        public void Terugdraaien()
        {
            if (tekenObjecten.Count > 0)
                tekenObjecten.RemoveAt(tekenObjecten.Count - 1);
        }

        public void verwijderBovensteObjectOpPunt(Point p)
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

        private static bool isRaak(TekenObject obj, Point p)
        {
            switch (obj.Tool)
            {
                case "tekst":

                    return true;
                case "kader":
                    return isOpKaderGeklikt(obj, p);
                case "vlak":
                    return isBinnenVierkant(obj, p, 0);
                case "cirkel":
                    return isEclipseRaakGeklikt(obj, p, false);
                case "rondje":
                    return isEclipseRaakGeklikt(obj, p, true);
                case "lijn":
                    return isOpLijnGeklikt(obj, p);
                case "pen":
                    return isOpPenGeklikt(obj, p);
            }
            return false;
        }

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

        // Bepaalt of het punt binnen het object ligt en geeft de mogelijkheid om de grootte van de rechthoek aan te passen
        private static bool isBinnenVierkant(TekenObject obj, Point p, int aanpassing)
        {
            return (p.X > obj.Points[0].X + aanpassing && p.X < obj.Points[1].X - aanpassing && 
                    p.Y > obj.Points[0].Y + aanpassing && p.Y < obj.Points[1].Y - aanpassing);
        }

        // Laat weten of er op de rand van de rechthoek geklikt is, het kader wordt aan weerskanten vergroot met de helft van de dikte van de gumrand
        private static bool isOpKaderGeklikt(TekenObject obj, Point p)
        {
            return (!isBinnenVierkant(obj, p, GumRandDikte / 2) && isBinnenVierkant(obj, p, -(GumRandDikte / 2)));
        }

        // Bepaalt of punt p binnen of op de lijn van de cirkel ligt afhankelijk van de waarde van de boolean isGevuldeCirkel
        private static bool isEclipseRaakGeklikt(TekenObject obj, Point p, bool isGevuldeCirkel)
        {
            Point begin = new Point(Math.Min(obj.Points[0].X, obj.Points[1].X), Math.Min(obj.Points[0].Y, obj.Points[1].Y));
            Point eind = new Point(Math.Max(obj.Points[0].X, obj.Points[1].X), Math.Max(obj.Points[0].Y, obj.Points[1].Y));
            double radiusx = Math.Abs((double)eind.X - (double)begin.X) / 2.0;
            double radiusy = Math.Abs((double)eind.Y - (double)begin.Y) / 2.0;
            double middelpuntx = begin.X + radiusx;
            double middelpunty = begin.Y + radiusy;

            if (isGevuldeCirkel)
                return isPuntBinnenCirkel(p, radiusx, radiusy, middelpuntx, middelpunty);
            else
                return isPuntOpRand(p, radiusx, radiusy, middelpuntx, middelpunty);
        }

        // Laat weten of er op de rand van de ellips geklikt is, de ellips wordt aan weerskanten vergroot met de helft van de dikte van de gumrand
        // Maakt gebruik van de stelling van pythagoras om de afstand te berekenen t.o.v. het middelpunt
        private static bool isPuntOpRand(Point p, double radiusx, double radiusy, double middelpuntx, double middelpunty)
        {
            return (Math.Sqrt(Math.Pow((p.X - middelpuntx) / (radiusx - GumRandDikte), 2) + Math.Pow((p.Y - middelpunty) / (radiusy - GumRandDikte), 2)) >= 1 
                && isPuntBinnenCirkel(p, radiusx, radiusy, middelpuntx, middelpunty));
        }

        // 
        private static bool isPuntBinnenCirkel(Point p, double radiusx, double radiusy, double middelpuntx, double middelpunty)
        {
            return (Math.Sqrt(Math.Pow((p.X - middelpuntx) / radiusx, 2) + Math.Pow((p.Y - middelpunty) / radiusy, 2)) <= 1);
        }

        // Deze methode is afgeleid van http://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line
        // en berekent de kortste afstand vanaf punt p tot aan de lijn die loop vanaf het punt begin tot het punt eind
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
