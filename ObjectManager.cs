using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace SchetsEditor
{
    public class ObjectManager
    {
        private List<DrawObject> objects = new List<DrawObject>();

        public List<DrawObject> getObjects
        {
            get { return objects; }
        }

        public void SerializeToXML(string filename)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<DrawObject>));
            StreamWriter writer = new StreamWriter(filename);
            serializer.Serialize(writer, this.objects);
            writer.Close();
        }

        public void DeserializeFromXML(string filename)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(List<DrawObject>));
            TextReader textReader = new StreamReader(filename);
            this.objects = (List<DrawObject>)deserializer.Deserialize(textReader);
            textReader.Close();
        }

        public void assignObject(DrawObject tekenObject)
        {
            objects.Add(tekenObject);
        }
    }

    public class DrawFromXML
    {
        public static void DrawingFromXML(Graphics gr, List<DrawObject> objects)
        {
            Font font = new Font("Tahoma", 40);

            foreach (DrawObject obj in objects)
            {
                Color color = Color.FromName(obj.Color);
                SolidBrush brush = new SolidBrush(color);
                Pen pen = new Pen(brush, 3);
                Rectangle rect = new Rectangle();

                if (obj.Tool == "kader" || obj.Tool == "vlak" || obj.Tool == "cirkel" || obj.Tool == "rondje")
                {
                    Point punt = new Point(Math.Min(obj.Points[0].X, obj.Points[1].X), Math.Min(obj.Points[0].Y, obj.Points[1].Y));
                    Size grootte = new Size(Math.Abs(obj.Points[0].X - obj.Points[1].X), Math.Abs(obj.Points[0].Y - obj.Points[1].Y));
                    rect = new Rectangle(punt, grootte);
                }

                switch (obj.Tool)
                {
                    case "tekst":
                        gr.DrawString(obj.Text, font, brush, obj.Points[0], StringFormat.GenericTypographic);
                        break;
                    case "kader":
                        gr.DrawRectangle(pen, rect);
                        break;
                    case "vlak":
                        gr.FillRectangle(brush, rect);
                        break;
                    case "cirkel":
                        gr.DrawEllipse(pen, rect);
                        break;
                    case "rondje":
                        gr.FillEllipse(brush, rect);
                        break;
                    case "lijn":
                        gr.DrawLine(pen, obj.Points[0], obj.Points[1]);
                        break;
                    case "pen":
                        for (int i = 1; i < obj.Points.Count; i++)
                            gr.DrawLine(pen, obj.Points[i - 1], obj.Points[i]);
                        break;
                }
            }
        }
    }

    public class DrawObject
    {
        public DrawObject()
        { Text = null; }

        public string Tool
        { get; set; }

        public List<Point> Points { get; set; }

        public string Color
        { get; set; }

        public string Text
        { get; set; }

        
    }
}
