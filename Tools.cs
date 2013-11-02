using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;

namespace SchetsEditor
{
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
        protected TekenObject obj;
        protected List<Point> points;
        protected ObjectControl objectmanager = new ObjectControl();

        public virtual void MuisVast(SchetsControl s, Point p)
        {
            kwast = new SolidBrush(Color.FromName((s.PenKleur).Name));
            startpunt = p;
            obj = new TekenObject();
            points = new List<Point>();
            objectmanager = s.GetManager;       

            objectmanager.objectToewijzen(obj);
            points.Add(p);
            obj.Kleur = (s.PenKleur).Name;
        }

        public virtual void MuisLos(SchetsControl s, Point p) { }

        public abstract void MuisDrag(SchetsControl s, Point p);
        public abstract void Letter(SchetsControl s, char c);

    }

    public class TekstTool : StartpuntTool
    {     
        public override string ToString() { return "tekst"; }

        public override void MuisVast(SchetsControl s, Point p)
        {
            base.MuisVast(s, p);
            obj.Tool = ToString();
            obj.Points = points;
        }

        public override void MuisDrag(SchetsControl s, Point p) { }

        public override void Letter(SchetsControl s, char c)
        {
            if (c == 8)
            {
                string res = "";
                for (int i = 0; i < (obj.Tekst.Length - 1); i++)
                    res += obj.Tekst[i];
                obj.Tekst = res;
                s.MaakBitmapGraphics().FillRectangle(Brushes.White, 0, 0, s.GetBitmap.Width, s.GetBitmap.Height);
            } else if (c >= 32) {
                obj.Tekst += c.ToString();
            }

            s.Invalidate();
            DrawFromXML.DrawingFromXML(s.MaakBitmapGraphics(), objectmanager.getObjects);
        }
    }

    public abstract class TweepuntTool : StartpuntTool
    {
        public static Pen MaakPen(Brush b, int dikte)
        {
            Pen pen = new Pen(b, dikte);
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            return pen;
        }

        public static Rectangle Punten2Rechthoek(Point p1, Point p2)
        {
            Point punt = new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
            Size grootte = new Size(Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y));
            return new Rectangle(punt, grootte);
        }

        public override void MuisVast(SchetsControl s, Point p)
        {
            base.MuisVast(s, p);
            obj.Tool = ToString();
        }

        public override void MuisDrag(SchetsControl s, Point p)
        {
            s.Refresh();
            this.Teken(s.CreateGraphics(), this.startpunt, p);
        }

        public override void MuisLos(SchetsControl s, Point p)
        {
            points.Add(p);

            obj.Points = points;

            
            s.Invalidate();
            DrawFromXML.DrawingFromXML(s.MaakBitmapGraphics(), objectmanager.getObjects);
        }

        public abstract void Teken(Graphics g, Point p1, Point p2);

        public abstract void Teken(Graphics g, Point p1, Point p2, Brush kwast);

        public override void Letter(SchetsControl s, char c) { }
    }

    public class RechthoekTool : TweepuntTool
    {
        public override string ToString() { return "kader"; }

        public override void Teken(Graphics g, Point p1, Point p2)
        {
            Teken(g, p1, p2, kwast);
        }

        public override void Teken(Graphics g, Point p1, Point p2, Brush kwast)
        {
            g.DrawRectangle(MaakPen(kwast, 3), TweepuntTool.Punten2Rechthoek(p1, p2));
        }

    }
    
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

    public class PenTool : LijnTool
    {
        public override string ToString() { return "pen"; }

        public override void MuisDrag(SchetsControl s, Point p)
        {
            points.Add(p);
            TekenLijn(s.CreateGraphics(), points, kwast);
        }

        public void TekenLijn(Graphics g, List<Point> points, Brush kwast)
        {
            for (int i = 1; i < points.Count; i++)
                g.DrawLine(MaakPen(kwast, 3), points[i - 1], points[i]);
        }
    }
    
    public class GumTool : PenTool
    {
        // Oude Gummert
    }
}
