using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace SchetsEditor
{
    public class TekenObject
    {
        public TekenObject()
        { Tekst = null; }

        public string Tool
        { get; set; }

        public List<Point> Points { get; set; }

        public string Kleur
        { get; set; }

        public string Tekst
        { get; set; }      
    }
}
