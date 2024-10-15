using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Asos.CodeTest;

public class DataDeserializer
{
    public static T Deserialize<T>(string data) where T : class
    {
        if (string.IsNullOrEmpty(data))
        {
            throw new ArgumentNullException(nameof(data), "Input data cannot be null or empty.");
        }

        try
        {
            var js = new DataContractJsonSerializer(typeof(T));
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(data));
            var output = js.ReadObject(ms) as T;

            return output ?? throw new InvalidOperationException("Deserialization returned null.");
        }
        catch (SerializationException ex)
        {
            throw new InvalidOperationException("Failed to deserialize the data.", ex);
        }
    }
}
