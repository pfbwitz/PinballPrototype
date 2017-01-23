using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DepthTracker.Tiles
{
    public static class TileExtensions
    {
        public static bool ContainsTile(this IEnumerable<Tile> tiles, Tile tile)
        {
            return tiles.Any(t => t.Col == tile.Col && t.Row == tile.Row);
        }

        public static List<Tile> AddIfNotExists(this List<Tile> tiles, Tile tile)
        {
            if (tile != null && !tiles.ContainsTile(tile))
                tiles.Add(tile);
            return tiles;
        }

        public static List<Tile> AddRangeIfNotExists(this List<Tile> tiles, IEnumerable<Tile> tilesToAdd)
        {
            foreach(var t in tilesToAdd)
                tiles = tiles.AddIfNotExists(t);
            
            return tiles;
        }

        public static bool IsPixelInTile(this Point pixel, Rectangle tile)
        {
            return tile.Contains(pixel);
        }

        public static Point GetCalibratedPointForPixel(this Point pixel, int tileDimensionsWidth, int tileDimensionsHeight)
        {
            for (var x = 1; x <= 4; x++)
            {
                for (var y = 1; y <= 2; y++)
                {
                    if (pixel.IsPixelInTile(new Rectangle(tileDimensionsWidth * x, tileDimensionsHeight * y,
                        tileDimensionsWidth, tileDimensionsHeight)))
                        return new Point(x, y);
                }
            }
            return new Point(0, 0);
        }
    }
}
