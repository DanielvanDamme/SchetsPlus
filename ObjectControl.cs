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
        const int GumRandDikte = 8;

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
                    return isOpPenGeklikt(obj, p);
            }
            return false;
        }

        private static bool isOpPenGeklikt(TekenObject obj, Point p)
        {
            for (int i = 1; i < obj.Points.Count; i++)
            {
                if (afstandTotLijn(obj.Points[i - 1], obj.Points[i], p) < GumRandDikte)
                    return true;
            }
            return false;
        }


        // Deze methode is afgeleid van http://en.wikipedia.org/wiki/Distance_from_a_point_to_a_line
        private static double afstandTotLijn(Point begin, Point eind, Point p)
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

        private static bool isOpLijnGeklikt(TekenObject obj, Point p)
        {
            return (afstandTotLijn(obj.Points[0], obj.Points[1], p) < GumRandDikte);
        }

        private static bool isBinnenVierkant(TekenObject obj, Point p, int aanpassing)
        {
            return (p.X > obj.Points[0].X + aanpassing && p.X < obj.Points[1].X - aanpassing && 
                    p.Y > obj.Points[0].Y + aanpassing && p.Y < obj.Points[1].Y - aanpassing);
        }

        private static bool isKaderRaakGeklikt(TekenObject obj, Point p)
        {
            return (!isBinnenVierkant(obj, p, GumRandDikte) && isBinnenVierkant(obj, p, 0));
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
            return (Math.Sqrt(Math.Pow((p.X - middelpuntx) / (radiusx - GumRandDikte), 2) + Math.Pow((p.Y - middelpunty) / (radiusy - GumRandDikte), 2)) >= 1 
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
            gr.FillRectangle(Brushes.White, 0, 0, 1000, 1000);
            Font font = new Font("Tahoma", 40);

            foreach (TekenObject obj in objects)
            {
                Color color = Color.FromName(obj.Kleur);
                SolidBrush brush = new SolidBrush(color);

                switch (obj.Tool)
                {
                    case "tekst":
                        
                        SizeF sz = gr.VisibleClipBounds.Size;
                        gr.TranslateTransform(sz.Width / 2, sz.Height / 2);
                        gr.RotateTransform(obj.Hoek);
                        sz = gr.MeasureString(obj.Tekst, font);
                        gr.DrawString(obj.Tekst, font, Brushes.Black, -(sz.Width / 2) + obj.Points[0].X, -(sz.Height / 2) + obj.Points[0].Y);
                        gr.ResetTransform();

                        //gr.DrawString(obj.Tekst, font, brush, obj.Points[0], StringFormat.GenericDefault);
                        //gr.ResetTransform();
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
