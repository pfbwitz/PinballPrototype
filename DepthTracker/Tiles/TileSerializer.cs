using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace DepthTracker.Tiles
{
    public static class TileSerializer
    {
        public static string Serialize(this List<Tile> tiles)
        {
            using (var ms = new MemoryStream())
            {
                new DataContractJsonSerializer(tiles.GetType()).WriteObject(ms, tiles);
                return Encoding.Default.GetString(ms.ToArray());
            }
        }

        public static List<Tile> Deserialize(this string json)
        {
            var coll = JsonConvert.DeserializeObject<List<Tile>>(json);
            return coll;
        }

        public static string Serialize(this Tile tile)
        {
            using (var ms = new MemoryStream())
            {
                new DataContractJsonSerializer(tile.GetType()).WriteObject(ms, tile);
                return Encoding.Default.GetString(ms.ToArray());
            }
        }

        public static Tile DeserializeTile(this string json)
        {
            var coll = JsonConvert.DeserializeObject<Tile>(json);
            return coll;
        }
    }
}
