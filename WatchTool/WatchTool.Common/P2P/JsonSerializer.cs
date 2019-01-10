using System;
using Newtonsoft.Json;

namespace WatchTool.Common.P2P
{
    public static class JsonSerializer
    {
        public static byte[] ToJson<T>(T obj) where T : class
        {
            string json = JsonConvert.SerializeObject(obj, Formatting.Indented);

            return Encoders.ASCII.DecodeData(json);
        }

        public static object FromJson(byte[] bytes, Type objectType)
        {
            string json = Encoders.ASCII.EncodeData(bytes);

            return JsonConvert.DeserializeObject(json, objectType);
        }
    }
}
