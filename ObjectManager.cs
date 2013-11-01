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

        public void assignObject(DrawObject tekenObject)
        {
            objects.Add(tekenObject);
        }

        public void SerializeToXML()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<DrawObject>));
            // StreamWriter is een speciaal soort TextWriter
            TextWriter writer = new StreamWriter(@"temp.xml");
            serializer.Serialize(writer, this.objects);
            writer.Close();
        }

        public List<DrawObject> DeserializeFromXML()
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(List<DrawObject>));
            TextReader reader = new StreamReader(@"temp.xml");
            List<DrawObject> objects = (List<DrawObject>)deserializer.Deserialize(reader);
            reader.Close();

            return objects;
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
                Size size = new Size(obj.Point2.X - obj.Point1.X, obj.Point2.Y - obj.Point1.Y);
                Point startPoint = obj.Point1;
                // Fix coords when dragging from Right to Left / Bottom to Top
                if(size.Width < 0)
                {
                    startPoint.X += size.Width;
                    size.Width = Math.Abs(size.Width);
                }
                if (size.Height < 0)
                {
                    startPoint.Y += size.Height;
                    size.Height = Math.Abs(size.Height);
                }
                Rectangle rect = new Rectangle(startPoint, size);

                switch (obj.Tool)
                {
                    case "tekst":
                        gr.DrawString(obj.Text, font, brush, obj.Point1, StringFormat.GenericTypographic);
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
                        gr.DrawLine(pen, obj.Point1, obj.Point2);
                        break;
                    case "pen":
                        gr.DrawLine(pen, obj.Point1, obj.Point2);
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

        public Point Point1
        { get; set; }

        public Point Point2
        { get; set; }

        public string Color
        { get; set; }

        public string Text
        { get; set; }
    }
}
