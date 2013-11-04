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
                if (p.X > tekenObjecten[i].Points[0].X && p.X < tekenObjecten[i].Points[1].X && p.Y > tekenObjecten[i].Points[0].Y && p.Y < tekenObjecten[i].Points[1].Y)
                    tekenObjecten.RemoveAt(i);
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
