using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;

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
        protected Point startpunt;
        protected DrawObject obj = new DrawObject();

        public virtual void MuisVast(SchetsControl s, Point p)
        {
            startpunt = p;
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
                obj.Point1 = startpunt;
                obj.Color = (s.PenKleur).Name;
                obj.Text = tekst;

                ObjectManager objectmanager = s.GetManager;
                objectmanager.assignObject(obj);

                // Roep REDRAW FUNCTIE AAN
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
        public override void MuisVast(SchetsControl s, Point p)
        {
            base.MuisVast(s, p);

            obj.Tool = ToString();
            obj.Point1 = p;
            obj.Color = (s.PenKleur).Name;
        }

        public override void MuisDrag(SchetsControl s, Point p) { }

        public override void MuisLos(SchetsControl s, Point p)
        {   
            base.MuisLos(s, p);

            obj.Point2 = p;

            ObjectManager objectmanager = s.GetManager;
            objectmanager.assignObject(obj);

            // Roep REDRAW FUNCTIE AAN
            Graphics gr = s.MaakBitmapGraphics();
            s.Invalidate();
            DrawFromXML.DrawingFromXML(gr, objectmanager.getObjects);
        }

        public override void Letter(SchetsControl s, char c) { }
    }

    public class RechthoekTool : TweepuntTool
    {
        public override string ToString() { return "kader"; }
    }
    
    public class VolRechthoekTool : RechthoekTool
    {
        public override string ToString() { return "vlak"; }
    }

    public class CirkelTool : TweepuntTool
    {
        public override string ToString() { return "cirkel"; }
    }

    public class RondjeTool : CirkelTool
    {
        public override string ToString() { return "rondje"; }
    }

    public class LijnTool : TweepuntTool
    {
        public override string ToString() { return "lijn"; }
    }

    public class PenTool : LijnTool
    {
        public override string ToString() { return "pen"; }

        public override void MuisDrag(SchetsControl s, Point p)
        {
            this.MuisLos(s, p);
            this.MuisVast(s, p);
        }
    }
    
    public class GumTool : PenTool
    {
        // Oude Gummert
    }
}
