using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

namespace DeeZEngine.Engine.Extensions
{

    public static class SerializerExtensions
    {

        public static string Serialize(this object _object, bool compress = false)
        {
            if (!compress) 
                return JsonConvert.SerializeObject(_object);

            string str = JsonConvert.SerializeObject(_object);
            byte[] bytes = Encoding.Unicode.GetBytes(str);
            using MemoryStream msBytes = new MemoryStream(bytes);
            using MemoryStream ms = new MemoryStream();
            using (GZipStream gs = new GZipStream(ms, CompressionMode.Compress)) {
                msBytes.CopyTo(gs);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        public static T Deserialize<T>(this string _jsonString, bool compress = false)
        {
            if(!compress)
                return JsonConvert.DeserializeObject<T>(_jsonString);

            byte[] bytes = Convert.FromBase64String(_jsonString);
            using MemoryStream msBytes = new MemoryStream(bytes);
            using MemoryStream ms = new MemoryStream();
            using (GZipStream gs = new GZipStream(msBytes, CompressionMode.Decompress)) {
                gs.CopyTo(ms);
            }
            return JsonConvert.DeserializeObject<T>(Encoding.Unicode.GetString(ms.ToArray()));
        }

    }

}
