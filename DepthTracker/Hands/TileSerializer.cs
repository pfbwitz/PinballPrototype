using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace DepthTracker.Hands
{
    public static class TileSerializer
    {
        public static string Serialize(this List<Tile> tiles)
        {
            var serializer = new DataContractJsonSerializer(tiles.GetType());

            using (var ms = new MemoryStream())
            {
                serializer.WriteObject(ms, tiles);

                return Encoding.Default.GetString(ms.ToArray());
            }
        }

        public static List<Tile> Deserialize(this string json)
        {
            var coll = JsonConvert.DeserializeObject<List<Tile>>(json);
            return coll;
        }
    }
}
