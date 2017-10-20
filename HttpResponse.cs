using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Xml;
using System.Xml.Serialization;

namespace thor
{
    public class HttpResponse
    {
        public static string JsonHttpResponseMessagePropertyName => "HttpResponseMessage";
        
        public HttpResponseMessage HttpResponseMessage { get; set; }
        public Stream ResponseContent { get; set; }
        
        public HttpResponse(HttpResponseMessage httpResponseMessage, Stream responseContent)
        {
            HttpResponseMessage = httpResponseMessage;
            ResponseContent = responseContent;
        }

        public T Json<T>() where T : class, new()
        {
            var responseContentStr = GetResponseContentString();
            if (string.IsNullOrWhiteSpace(responseContentStr))
            {
                return new T();
            }
            
            ResponseContent.Position = 0;
            var deSerializer = new DataContractJsonSerializer(typeof(T),
                new DataContractJsonSerializerSettings {UseSimpleDictionaryFormat = true});
            var responseModel = (T) deSerializer.ReadObject(ResponseContent);

            var httpResponseProperty = responseModel.GetType().GetRuntimeProperties()
                .FirstOrDefault(p => p.Name.Equals(JsonHttpResponseMessagePropertyName));
            httpResponseProperty?.SetValue(responseModel, HttpResponseMessage);

            return responseModel;
        }
        
        public T Xml<T>() where T : class, new()
        {
            var responseContentStr = GetResponseContentString();
            if (string.IsNullOrWhiteSpace(responseContentStr))
            {
                return new T();
            }
            
            ResponseContent.Position = 0;
            var deSerializer = new XmlSerializer(typeof(T));
            var responseModel = (T) deSerializer.Deserialize(ResponseContent);

            return responseModel;
        }

        public XmlDocument Xml()
        {
            var xmlString = GetResponseContentString();
            
            var document = new XmlDocument();
            document.LoadXml(xmlString);

            return document;
        }

        public string GetResponseContentString()
        {
            ResponseContent.Position = 0;
            var reader = new StreamReader(ResponseContent);

            return reader.ReadToEnd();
        }
    }
}