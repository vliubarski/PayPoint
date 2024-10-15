using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Asos.CodeTest
{
    public class DataDeserializer
    {
        public static T Deserialize<T>(string data) where T : class
        {
            var js = new DataContractJsonSerializer(typeof(T));

            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(data)))
            {
                var output = js.ReadObject(ms) as T;
                return output;
            }
        }
    }
}