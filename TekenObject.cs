using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;

namespace SchetsEditor
{
    public class TekenObject
    {
        public string Tool
        { get; set; }

        public List<Point> Points 
        { get; set; }

        public string Kleur
        { get; set; }

        [DefaultValue("")]
        public string Tekst
        { get; set; }
        
        [DefaultValue(0)]
        public int Hoek
        { get; set; }
    }
}
