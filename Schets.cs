using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace SchetsEditor
{
    class Schets
    {
        private Bitmap bitmap;
        
        // 2: aangepaste constructor
        public Schets(string bestandsLocatie)
        {
            bitmap = new Bitmap(1, 1);
            openBestand(bestandsLocatie);
        }
        // 2 constructor overloading
        public Schets() : this("")
        {
        }
        // 2: Property to get the drawing
        public Bitmap GetBitmap
        {
            get { return bitmap; }
        }
        public Graphics BitmapGraphics
        {
            get { return Graphics.FromImage(bitmap); }
        }

        //2
        private void openBestand(string bestandsLocatie)
        {
            if (bestandsLocatie != "")
            {
                bitmap = new Bitmap(bestandsLocatie);
            }
        }

        public void VeranderAfmeting(Size sz)
        {
            if (sz.Width > bitmap.Size.Width || sz.Height > bitmap.Size.Height)
            {
                Bitmap nieuw = new Bitmap( Math.Max(sz.Width,  bitmap.Size.Width)
                                         , Math.Max(sz.Height, bitmap.Size.Height)
                                         );
                Graphics gr = Graphics.FromImage(nieuw);
                gr.FillRectangle(Brushes.White, 0, 0, sz.Width, sz.Height);
                gr.DrawImage(bitmap, 0, 0);
                bitmap = nieuw;
            }
        }
        public void Teken(Graphics gr)
        {
            gr.DrawImage(bitmap, 0, 0);
        }
        public void Schoon()
        {
            Graphics gr = Graphics.FromImage(bitmap);
            gr.FillRectangle(Brushes.White, 0, 0, bitmap.Width, bitmap.Height);
        }
        public void Roteer()
        {
            bitmap.RotateFlip(RotateFlipType.Rotate90FlipNone);
        }
    }
}
