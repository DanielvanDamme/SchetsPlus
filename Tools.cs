using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Collections.Generic;
using System.Windows;

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
        protected Pen pen;
        protected Brush kwast;
        protected Color kleur;
        protected Point startpunt;
        protected DrawObject obj;
        protected List<Point> points;

        public virtual void MuisVast(SchetsControl s, Point p)
        {
            points = new List<Point>();
            obj = new DrawObject();
            startpunt = p;
            kleur = Color.FromName((s.PenKleur).Name);
            kwast = new SolidBrush(kleur);
            pen = new Pen(kwast, 3);
        }

        public virtual void MuisLos(SchetsControl s, Point p) { }

        public abstract void MuisDrag(SchetsControl s, Point p);
        public abstract void Letter(SchetsControl s, char c);

    }

    public class TekstTool : StartpuntTool
    {
        public override string ToString() { return "tekst"; }

        public override void MuisDrag(SchetsControl s, Point p) { }

        public override void Letter(SchetsControl s, char c)
        {
            if (c >= 32)
            {
                string tekst = c.ToString();

                obj.Tool = ToString();
                points.Add(startpunt);
                obj.Points = points;
                obj.Color = kleur.Name;
                obj.Text = tekst;

                ObjectManager objectmanager = s.GetManager;
                objectmanager.assignObject(obj);

                Graphics gr = s.MaakBitmapGraphics();
                Font font = new Font("Tahoma", 40);
                SizeF sz = gr.MeasureString(tekst, font, this.startpunt, StringFormat.GenericTypographic);
                s.Invalidate();
                DrawFromXML.DrawingFromXML(gr, objectmanager.getObjects);
                startpunt.X += (int)sz.Width;
            }
        }
    }

    public abstract class TweepuntTool : StartpuntTool
    {
        public static Rectangle Punten2Rechthoek(Point p1, Point p2)
        {
            Point punt = new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y));
            Size grootte = new Size(Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y));
            return new Rectangle(punt, grootte);
        }

        public override void MuisVast(SchetsControl s, Point p)
        {
            base.MuisVast(s, p);

            points.Add(p);

            obj.Tool = ToString();
            obj.Color = (s.PenKleur).Name;
        }

        public override void MuisDrag(SchetsControl s, Point p)
        {
            s.Refresh();
            this.Bezig(s.CreateGraphics(), this.startpunt, p);
        }

        public override void MuisLos(SchetsControl s, Point p)
        {
            points.Add(p);

            obj.Points = points;

            ObjectManager objectmanager = s.GetManager;
            objectmanager.assignObject(obj);
            s.Refresh();
            s.Invalidate();
            DrawFromXML.DrawingFromXML(s.MaakBitmapGraphics(), objectmanager.getObjects);
        }

        public abstract void Bezig(Graphics g, Point p1, Point p2);

        public override void Letter(SchetsControl s, char c) { }
    }

    public class RechthoekTool : TweepuntTool
    {
        public override string ToString() { return "kader"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.DrawRectangle(pen, TweepuntTool.Punten2Rechthoek(p1, p2));
        }
    }
    
    public class VolRechthoekTool : RechthoekTool
    {
        public override string ToString() { return "vlak"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.FillRectangle(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
        }
    }

    public class CirkelTool : TweepuntTool
    {
        public override string ToString() { return "cirkel"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.DrawEllipse(pen, TweepuntTool.Punten2Rechthoek(p1, p2));
        }
    }

    public class RondjeTool : CirkelTool
    {
        public override string ToString() { return "rondje"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.FillEllipse(kwast, TweepuntTool.Punten2Rechthoek(p1, p2));
        }
    }

    public class LijnTool : TweepuntTool
    {
        public override string ToString() { return "lijn"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.DrawLine(pen, p1.X, p1.Y, p2.X, p2.Y);
        }
    }

    public class PenTool : LijnTool
    {
        public override string ToString() { return "pen"; }

        public override void MuisDrag(SchetsControl s, Point p)
        {
            base.MuisDrag(s, p);
            points.Add(p);
        }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            for (int i = 1; i < points.Count; i++)
                g.DrawLine(pen, points[i - 1], points[i]);
        }
    }
    
    public class GumTool : PenTool
    {
        // Oude Gummert
    }
}
