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

        public void SerializeToXML()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<DrawObject>));
            StreamWriter writer = new StreamWriter(@"temp.xml");
            serializer.Serialize(writer, this.objects);
            writer.Close();
        }

        public void assignObject(DrawObject tekenObject)
        {
            objects.Add(tekenObject);
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
