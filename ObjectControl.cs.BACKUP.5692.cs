using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace SchetsEditor
{
    public class ObjectControl
    {
        private List<TekenObject> tekenObjecten;

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

        public void Terugdraaien()
        {
            if (tekenObjecten.Count > 0)
                tekenObjecten.RemoveAt(tekenObjecten.Count - 1);
        }

        public void Toewijzen(TekenObject tekenObject)
        {
            tekenObjecten.Add(tekenObject);
        }

        // Verplaats Serialize en Deserialize naar opslaan
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
        
        public void objectVerwijderen(Point p)
        {
            for (int i = (tekenObjecten.Count - 1); i >= 0; i--)
            {
<<<<<<< HEAD
                if (p.X > objecten[i].Points[0].X && p.X < objecten[i].Points[1].X && p.Y > objecten[i].Points[0].Y && p.Y < objecten[i].Points[1].Y)
                    objecten.RemoveAt(i);
=======
                if (isRaak(objecten[i], p))
                    tekenObjecten.RemoveAt(i);
            }
        private bool isRaak(TekenObject obj, Point p)
        {
            switch (obj.Tool)
            {
                case "tekst":

                    return true;
                case "kader":

                    return true;
                case "vlak":
                    if (p.X > obj.Points[0].X && p.X < obj.Points[1].X && p.Y > obj.Points[0].Y && p.Y < obj.Points[1].Y)
                        return true;
                    break;
                case "cirkel":
                    return eclipseRaak(obj, p, false);
                case "rondje":
                    return eclipseRaak(obj, p, true);
                    break;
                case "lijn":

                    return true;
                case "pen":

                    return true;
            }
            return false;
        }

        private bool eclipseRaak(TekenObject obj, Point p, bool vollecirkel)
        {
            Point p1 = new Point(Math.Min(obj.Points[0].X, obj.Points[1].X), Math.Min(obj.Points[0].Y, obj.Points[1].Y));
            Point p2 = new Point(Math.Max(obj.Points[0].X, obj.Points[1].X), Math.Max(obj.Points[0].Y, obj.Points[1].Y));
            double radiusx = Math.Abs((double)p2.X - (double)p1.X) / 2.0;
            double radiusy = Math.Abs((double)p2.Y - (double)p1.Y) / 2.0;
            double middelpuntx = p1.X + radiusx;
            double middelpunty = p1.Y + radiusy;

            if (vollecirkel)
            {
                return helecirkel(p, radiusx, radiusy, middelpuntx, middelpunty);
            }
            else
            {
                return cirkelrand(p, radiusx, radiusy, middelpuntx, middelpunty);
>>>>>>> 32e50d0a28f441719f89852fda27b4a472bdbe7a
            }
        }

        private bool cirkelrand(Point p, double radiusx, double radiusy, double middelpuntx, double middelpunty)
        {
            const int PenSize = 3;
            if (Math.Sqrt(Math.Pow((p.X - middelpuntx) / (radiusx - PenSize), 2) + Math.Pow((p.Y - middelpunty) / (radiusy - PenSize), 2)) >= 1 &&
                helecirkel(p, radiusx, radiusy, middelpuntx, middelpunty))
                return true;
            else
                return false;
        }

        private bool helecirkel(Point p, double radiusx, double radiusy, double middelpuntx, double middelpunty)
        {
            if (Math.Sqrt(Math.Pow((p.X - middelpuntx) / radiusx, 2) + Math.Pow((p.Y - middelpunty) / radiusy, 2)) <= 1)
                return true;
            else
                return false;
        }



        
        

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
