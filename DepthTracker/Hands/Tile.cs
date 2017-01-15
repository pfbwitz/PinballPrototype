using System.Windows;
using System.Runtime.Serialization;

namespace DepthTracker.Hands
{
    [DataContract]
    public class Tile
    {
        [DataMember(Name = "touch")]
        public bool Touch { get; private set; }

        [DataMember(Name = "row")]
        public int Row { get; private set; }

        [DataMember(Name = "col")]
        public int Col { get; private set; }

        private Tile(bool touch, int row, int col)
        {
            Touch = touch;
            Row = row;
            Col = col;
        }

        public static Tile GetInstanceForPixel(Point pixel, Point bitmapDimensions, bool isGestureRecognized)
        {
            var tileWidth = (int)bitmapDimensions.X / 4;
            var tileHeight = (int)bitmapDimensions.Y / 2;
            for(var x = 0; x < 4; x++)
            {
                for(var y = 0; y < 2; y++)
                {
                    if (IsPixelInTile(pixel, new Int32Rect((int)tileWidth * x, (int)tileHeight * y, tileWidth, tileHeight)))
                        return new Tile(isGestureRecognized, y, x);
                }
            }
            return null;
        }

        public static bool IsPixelInTile(Point pixel, Int32Rect tile)
        {
            var xBounds = tile.X + tile.Width;
            var yBounds = tile.Y + tile.Height;
            return pixel.X >= tile.X && 
                pixel.X <= xBounds && 
                pixel.Y >= tile.Y && 
                pixel.Y <= yBounds;
        }
    }
}
