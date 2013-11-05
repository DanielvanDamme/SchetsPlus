using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;

namespace SchetsEditor
{
    // De Tools worden zowel gebruikt voor het 'live' tekenen als voor het inladen en tekenen van de data in een SchetsPlus XML 
    public interface ISchetsTool
    {
        void MuisVast(SchetsControl s, Point p);
        void MuisDrag(SchetsControl s, Point p);
        void MuisLos(SchetsControl s, Point p);
        void Letter(SchetsControl s, char c);
    }

    public abstract class StartpuntTool : ISchetsTool
    {
        protected Brush kwast;
        protected Point startpunt;
        protected TekenObject tekenObject;
        protected ObjectControl objectcontrol;

        /* Bij het indrukken van de muis wordt ObjectControl aangeroepen en wordt er een nieuw TekenObject aangemaakt dat direct
         * toegewezen wordt aan de lijst met getekende objecten. Het nieuwe TekenObject bevat al meteen de kleur van de kwast en 
         het startpunt */
        public virtual void MuisVast(SchetsControl s, Point p)
        {
            kwast = new SolidBrush(s.PenKleur);
            startpunt = p;
            tekenObject = new TekenObject();
            objectcontrol = s.GetController;

            objectcontrol.Toewijzen(tekenObject);
            tekenObject.Points.Add(p);
            tekenObject.Kleur = (s.PenKleur).Name;
        }

        public virtual void MuisLos(SchetsControl s, Point p) { }

        public abstract void MuisDrag(SchetsControl s, Point p);
        public abstract void Letter(SchetsControl s, char c);
    }

    /* De TekstTool registreert na het aanklikken van een startpunt alle toetsenbordinvoer van de gebruiker en slaat deze als string op,
     * opeenvolgende letters zitten dus in hetzelfde TekenObject */
    public class TekstTool : StartpuntTool
    {     
        public override string ToString() { return "tekst"; }

        public override void MuisVast(SchetsControl s, Point p)
        {
            base.MuisVast(s, p);
            tekenObject.Tool = ToString();
        }

        public override void MuisDrag(SchetsControl s, Point p) { }

        // Onderstaande methode handelt de toetsenbordinvoer af
        public override void Letter(SchetsControl s, char c)
        {
            // ASCII 8 = Backspace, het is mogelijk om tijdens het typen van een Tekst TekenObject te corrigeren door op [backspace] te drukken
            if (c == 8)
            {
                string res = "";
                for (int i = 0; i < (tekenObject.Tekst.Length - 1); i++)
                    res += tekenObject.Tekst[i];
                tekenObject.Tekst = res;
            }
            // Als een zichtbaar teken is ingedrukt (ASCII > 32) wordt deze toegevoegd aan de string 
            else if (c >= 32)
            { tekenObject.Tekst += c.ToString(); }

            // Invalidate() de schets en update de tekening
            s.Invalidate();
            Schets.Teken(s.MaakBitmapGraphics(), objectcontrol.Ophalen);
        }
    }

    // De TweepuntTool wordt door vrijwel alle Tools gebruikt (vlak, kader, cirkel, etc.) en bevat een aantal handige functies
    public abstract class TweepuntTool : StartpuntTool
    {
        private bool wasMuisVast = false;
        // Maak een pen aan op basis van de kwast
        public static Pen MaakPen(Brush b, int dikte)
        {
            Pen pen = new Pen(b, dikte);
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            return pen;
        }

        // Op basis van 2 punten wordt een rechthoek geconstrueerd
        public static Rectangle Punten2Rechthoek(Point p1, Point p2)
        {
            Point punt = new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
            Size grootte = new Size(Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y));
            return new Rectangle(punt, grootte);
        }

        // Bij muis indrukken naam van de Tool opslaan
        public override void MuisVast(SchetsControl s, Point p)
        {
            base.MuisVast(s, p);
            wasMuisVast = true;
            tekenObject.Tool = ToString();
        }

        // Bij slepen met de muis de schets verversen en het object tonen dat getekend wordt 
        public override void MuisDrag(SchetsControl s, Point p)
        {
            s.Refresh();
            this.Teken(s.CreateGraphics(), this.startpunt, p);
        }

        // Bij loslaten muis het laatste punt toevoegen aan het TekenObject (deze is hiermee afgerond) en de tekening updaten
        public override void MuisLos(SchetsControl s, Point p)
        {
            if (wasMuisVast)
            {
                tekenObject.Points.Add(p);
                s.Invalidate();
                Schets.Teken(s.MaakBitmapGraphics(), objectcontrol.Ophalen);
                wasMuisVast = false;
            }
        }

        public abstract void Teken(Graphics g, Point p1, Point p2);

        public abstract void Teken(Graphics g, Point p1, Point p2, Brush kwast);

        public override void Letter(SchetsControl s, char c) { }
    }

    // RechthoekTool voor het tekenen van kaders
    public class RechthoekTool : TweepuntTool
    {
        public override string ToString() { return "kader"; }

        // Methode die aangeroepen wordt wanneer er 'live' getekend wordt
        public override void Teken(Graphics g, Point p1, Point p2)
        {
            Teken(g, p1, p2, kwast);
        }

        // Overloading methode die aangeroepen wordt wanneer de tekening opgebouwd wordt op basis van de TekenObjecten
        public override void Teken(Graphics g, Point p1, Point p2, Brush kwast)
        {
            g.DrawRectangle(MaakPen(kwast, 3), TweepuntTool.Punten2Rechthoek(p1, p2));
        }

    }

    // Uitbreiding op de RechthoekTool voor het tekenen van vlakken    
    public class VolRechthoekTool : RechthoekTool
    {
        public override string ToString() { return "vlak"; }

        public override void Teken(Graphics g, Point p1, Point p2)
        {
            Teken(g, p1, p2, kwast);
        }

        public override void Teken(Graphics g, Point p1, Point p2, Brush kwast)
        {
            g.FillRectangle(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
        }
    }

    // CirkelTool voor het tekenen van cirkels
    public class CirkelTool : TweepuntTool
    {
        public override string ToString() { return "cirkel"; }

        public override void Teken(Graphics g, Point p1, Point p2)
        {
            Teken(g, p1, p2, kwast);
        }

        public override void Teken(Graphics g, Point p1, Point p2, Brush kwast)
        {
            g.DrawEllipse(MaakPen(kwast, 3), TweepuntTool.Punten2Rechthoek(p1, p2));
        }

    }

    // Uitbreiding op de CirkelTool voor het tekenen van opgevulde rondjes
    public class RondjeTool : CirkelTool
    {
        public override string ToString() { return "rondje"; }

        public override void Teken(Graphics g, Point p1, Point p2)
        {
            Teken(g, p1, p2, kwast);
        }

        public override void Teken(Graphics g, Point p1, Point p2, Brush kwast)
        {
            g.FillEllipse(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
        }
    }

    // LijnTool voor het tekenen van rechte lijnen
    public class LijnTool : TweepuntTool
    {
        public override string ToString() { return "lijn"; }

        public override void Teken(Graphics g, Point p1, Point p2)
        {
            Teken(g, p1, p2, kwast);
        }

        public override void Teken(Graphics g, Point p1, Point p2, Brush kwast)
        {
            g.DrawLine(MaakPen(kwast, 3), p1, p2);
        }
    }

    /* PenTool, uitbreiding op de lijntool, vrije vormen getekend met de PenTool bestaan eigenlijk uit een heleboel punten binnen
     * één TekenObject die verbonden zijn met lijnen */
    public class PenTool : LijnTool
    {
        public override string ToString() { return "pen"; }

        /* Tijdens het slepen met de muis wordt constant een nieuw punt toegevoegd aan het TekenObject zodat de getrokken lijn 
         * later exact geconstrueerd kan worden */
        public override void MuisDrag(SchetsControl s, Point p)
        {
            tekenObject.Points.Add(p);
            TekenLijn(s.CreateGraphics(), tekenObject.Points, kwast);
        }

        // TekenLijn functie die simpelweg alle punten aan elkaar vastknoopt
        public void TekenLijn(Graphics g, List<Point> points, Brush kwast)
        {
            for (int i = 1; i < points.Count; i++)
                g.DrawLine(MaakPen(kwast, 3), points[i - 1], points[i]);
        }
    }

    // De nieuwe gumtool die lagen (TekenObjecten met een enkele muisklik kan verwijderen    
    public class GumTool : ISchetsTool
    {
        protected ObjectControl objectcontrol;

        public void Letter(SchetsControl s, char c) { }
        public void MuisDrag(SchetsControl s, Point p) { }
        public void MuisLos(SchetsControl s, Point p) { }

        // Nodig voor de goede weergave van het gumplaatje onder Tools
        public override string ToString() { return "gum"; }

        // Bij het klikken met de muis...
        public void MuisVast(SchetsControl s, Point p)
        {
            // ... wordt via de ObjectControl gekeken of er zich in de buurt van de klik objecten bevinden die uitgewist kunnen worden
            objectcontrol = s.GetController;
            objectcontrol.VerwijderBovensteObjectOpPunt(p);

            // Vervolgens wordt de tekening weer geupdate
            s.Invalidate();
            Schets.Teken(s.MaakBitmapGraphics(), objectcontrol.Ophalen);
        }
    }
}
