using System.IO;
using System.Runtime.Serialization.Json;

namespace WatchTool.Common.P2P
{
    public static class JsonSerializer
    {
        public static byte[] ToJson<T>(T obj) where T : class
        {
            using (MemoryStream stream = new MemoryStream())
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
                ser.WriteObject(stream, obj);

                stream.Position = 0;

                byte[] bytes;

                using (BinaryReader br = new BinaryReader(stream))
                {
                    bytes = br.ReadBytes((int)stream.Length);
                }

                return bytes;
            }
        }

        public static T FromJson<T>(byte[] bytes)
        {
            using (var ms = new MemoryStream(bytes))
            {
                DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(T));
                T deserialized = (T)deserializer.ReadObject(ms);
                return deserialized;
            }
        }
    }
}
