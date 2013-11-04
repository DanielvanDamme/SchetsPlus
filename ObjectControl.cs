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
        const int RandDikte = 3;

        public ObjectControl()
        {
            Reset();
        }

        public List<TekenObject> Ophalen
        {
            get
            {
                return tekenObjecten;
            }
        }

        public void Reset()
        {
            tekenObjecten = new List<TekenObject>();
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
                    return isKaderRaakGeklikt(obj, p);
                case "vlak":
                    return isBinnenVierkant(obj, p, 0);
                case "cirkel":
                    return isEclipseRaakGeklikt(obj, p, false);
                case "rondje":
                    return isEclipseRaakGeklikt(obj, p, true);
                case "lijn":
                    return isOpLijnGeklikt(obj, p);
                case "pen":

                    return true;
            }
            return false;
        }

        // Deze methode is afkomstig van
        // http://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line
        private static bool isOpLijnGeklikt(TekenObject obj, Point p)
        {
            Point begin = obj.Points[0];
            Point eind = obj.Points[1];
            int dx = eind.X - begin.X;
            int dy = eind.Y - begin.Y;

            // y = ax + b
            // a is de richtingscoefficient van de lijn (delta Y / delta X)
            int a = dy / dx;

            // om b te berekenen gebruiken we: b = y - ax
            int b = begin.Y - a * begin.X;

            double res = ((p.X - begin.X) * dx + (p.Y - begin.Y) * dy / (dx * dx + dy * dy));

            if (res > 1)
                res = 1;
            else if (res < 0)
                res = 0;



            double num = Math.Abs(p.Y - a * p.X - b);
            double denum = Math.Sqrt(a * a + 1);
            return (num / denum < 3);
        }

        private static bool isBinnenVierkant(TekenObject obj, Point p, int aanpassing)
        {
            return (p.X > obj.Points[0].X + aanpassing && p.X < obj.Points[1].X - aanpassing && 
                    p.Y > obj.Points[0].Y + aanpassing && p.Y < obj.Points[1].Y - aanpassing);
        }

        private static bool isKaderRaakGeklikt(TekenObject obj, Point p)
        {
            return (!isBinnenVierkant(obj, p, RandDikte) && isBinnenVierkant(obj, p, 0));
        }

        private static bool isEclipseRaakGeklikt(TekenObject obj, Point p, bool vollecirkel)
        {
            Point p1 = new Point(Math.Min(obj.Points[0].X, obj.Points[1].X), Math.Min(obj.Points[0].Y, obj.Points[1].Y));
            Point p2 = new Point(Math.Max(obj.Points[0].X, obj.Points[1].X), Math.Max(obj.Points[0].Y, obj.Points[1].Y));
            double radiusx = Math.Abs((double)p2.X - (double)p1.X) / 2.0;
            double radiusy = Math.Abs((double)p2.Y - (double)p1.Y) / 2.0;
            double middelpuntx = p1.X + radiusx;
            double middelpunty = p1.Y + radiusy;

            if (vollecirkel)
            {
                return isPuntBinnenCirkel(p, radiusx, radiusy, middelpuntx, middelpunty);
            }
            else
            {
                return isPuntOpRand(p, radiusx, radiusy, middelpuntx, middelpunty);
            }
        }

        private static bool isPuntOpRand(Point p, double radiusx, double radiusy, double middelpuntx, double middelpunty)
        {
            return (Math.Sqrt(Math.Pow((p.X - middelpuntx) / (radiusx - RandDikte), 2) + Math.Pow((p.Y - middelpunty) / (radiusy - RandDikte), 2)) >= 1 
                && isPuntBinnenCirkel(p, radiusx, radiusy, middelpuntx, middelpunty));
        }

        private static bool isPuntBinnenCirkel(Point p, double radiusx, double radiusy, double middelpuntx, double middelpunty)
        {
            return (Math.Sqrt(Math.Pow((p.X - middelpuntx) / radiusx, 2) + Math.Pow((p.Y - middelpunty) / radiusy, 2)) <= 1);
        }

        public void Terugdraaien()
        {
            if (tekenObjecten.Count > 0)
                tekenObjecten.RemoveAt(tekenObjecten.Count - 1);
        }
    }

    public class DrawFromXML
    {
        public static void DrawingFromXML(Graphics gr, List<TekenObject> objects)
        {
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
        }
    }
}
