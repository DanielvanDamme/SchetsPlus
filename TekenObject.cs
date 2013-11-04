using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;

namespace SchetsEditor
{
    /* Alle getekende objecten worden opgeslagen als TekenObject, op deze manier kunnen ze vervolgens eenvoudig
     * omgezet worden in een XML-bestand */
    public class TekenObject
    {
        public TekenObject()
        { Points = new List<Point>(); }

        public string Tool
        { get; set; }

        public List<Point> Points 
        { get; set; }

        public string Kleur
        { get; set; }

        /* Als onderstaande velden de DefaulftValue hebben zijn ze niet aanwezig in de XML, zo voorkomen we dat vierkanten, cirkels en
         * alle andere niet-tekstobjecten deze velden bevatten, zo besparen we ruimte in het XML-bestand */
        [DefaultValue("")]
        public string Tekst
        { get; set; }
        
        [DefaultValue(0)]
        public int Hoek
        { get; set; }
    }
}
