using System.IO;
using System.Runtime.Serialization.Json;

namespace Xamarin.Forms.HotReload.Extension.Helpers
{
    public class JsonSerializer
    {
        public T Deserialize<T>(string serializedString)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);
            writer.Write(serializedString);
            writer.Flush();
            memoryStream.Position = 0;
            using (memoryStream)
            {
                return (T) serializer.ReadObject(memoryStream);
            }
        }

        public string Serialize<T>(T serializingObject)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            var memoryStream = new MemoryStream();
            using (memoryStream)
            {
                serializer.WriteObject(memoryStream, serializingObject);
                memoryStream.Position = 0;
                var streamReader = new StreamReader(memoryStream);
                return streamReader.ReadToEnd();
            }
        }
    }
}