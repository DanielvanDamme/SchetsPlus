using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace SchetsEditor
{
    public class ObjectControl
    {
        private List<TekenObject> objecten = new List<TekenObject>();

        public List<TekenObject> getObjects
        {
            get { return objecten; }
        }

        public void SerializeToXML(string bestandsnaam)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<TekenObject>));
            StreamWriter writer = new StreamWriter(bestandsnaam);
            serializer.Serialize(writer, this.objecten);
            writer.Close();
        }

        public void DeserializeFromXML(string bestandsnaam)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(List<TekenObject>));
            TextReader textReader = new StreamReader(bestandsnaam);
            this.objecten = (List<TekenObject>)deserializer.Deserialize(textReader);
            textReader.Close();
        }
        
        public void objectToewijzen(TekenObject tekenObject)
        {
            objecten.Add(tekenObject);
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
                        gr.DrawString(obj.Tekst, font, brush, obj.Points[0], StringFormat.GenericTypographic);
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
