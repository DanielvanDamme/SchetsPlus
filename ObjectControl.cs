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
}
