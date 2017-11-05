using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Brthor.Http
{
    public static class JsonStringExtensions
    {
        public static T Json<T>(this string jsonString) where T : class, new()
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return new T();
            }
            
            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(jsonString)))
            {
                var deSerializer = new DataContractJsonSerializer(typeof(T),
                    new DataContractJsonSerializerSettings {UseSimpleDictionaryFormat = true});
                
                var responseModel = (T) deSerializer.ReadObject(ms);
                return responseModel;
            }
        }
    }
}