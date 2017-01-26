using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace DepthTracker.EmuCV
{
    public static class ShapeHelper
    {
        public static List<MCvBox2D> GetRectangles(WriteableBitmap bitmap)
        {
            var b = BitmapFromWriteableBitmap(bitmap);
            var image = new Image<Bgr, byte>(b);
            var gray = image.Convert<Gray, Byte>().PyrDown().PyrUp();

            var cannyThreshold = new Gray(180);
            var cannyThresholdLinking = new Gray(120);

            var cannyEdges = gray.Canny(cannyThreshold, cannyThresholdLinking);
            var lines = cannyEdges.HoughLinesBinary(1, Math.PI / 45.0, 20, 30, 10)[0];

            var boxList = new List<MCvBox2D>();

            using (var storage = new MemStorage())
                for (var contours = cannyEdges.FindContours(); contours != null; contours = contours.HNext)
                {
                    var currentContour = contours.ApproxPoly(contours.Perimeter * 0.05, storage);

                    if (contours.Area > 250)
                    {
                        if (currentContour.Total == 4)
                        {
                            bool isRectangle = true;
                            var pts = currentContour.ToArray();
                            LineSegment2D[] edges = Emgu.CV.PointCollection.PolyLine(pts, true);
                            //using edges i found coordinates.
                            for (int i = 0; i < edges.Length; i++)
                            {
                                double angle = Math.Abs(
                                   edges[(i + 1) % edges.Length].GetExteriorAngleDegree(edges[i]));
                                if (angle < 80 || angle > 100)
                                {
                                    isRectangle = false;
                                    break;
                                }
                                if (isRectangle) boxList.Add(currentContour.GetMinAreaRect());
                            }
                        }
                    }
                }

            return boxList;
        }

        public static bool GetRectangle(WriteableBitmap bitmap, out Rectangle? r)
        {
            r = null;
            try
            {
                var boxList = GetRectangles(bitmap);

                if (!boxList.Any())
                    return false;

                var list = new List<Single>();
                foreach (var bo in boxList.Where(box => !box.size.IsEmpty))
                    list.Add(bo.size.Width * bo.size.Height);

                var smallest = boxList[list.IndexOf(list.Min())];

                //this stuff is reversed for some reason
                var width = smallest.size.Width;
                var height = smallest.size.Height;
                var center = smallest.center;

                var topLeft = new Point((int)(center.X - width / 2), (int)(center.Y - height / 2));
                var bottomRight = new Point((int)(center.X + width / 2), (int)(center.Y + height / 2));

                r = new Rectangle(topLeft.X, topLeft.Y, bottomRight.X - topLeft.X, bottomRight.Y - topLeft.Y);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static Bitmap BitmapFromWriteableBitmap(WriteableBitmap writeBmp)
        {
            Bitmap bmp;
            using (var outStream = new MemoryStream())
            {
                var enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create((BitmapSource)writeBmp));
                enc.Save(outStream);
                bmp = new Bitmap(outStream);
            }
            return bmp;
        }
    }
}
