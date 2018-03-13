using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace Brthor.Http
{
    public static class HttpUtilities
    {
        public static HttpClient ConstructHttpClientWithHeaders(Dictionary<string, string> customHeaders, HttpClient baseClient=null)
        {
            var loggingHandler = new LoggingHandler(new HttpClientHandler());
            var client = baseClient ?? new HttpClient(loggingHandler);

            if (customHeaders == null) 
                return client;
            
            foreach (var kvp in customHeaders)
            {
                if (client.DefaultRequestHeaders.Contains(kvp.Key)) client.DefaultRequestHeaders.Remove(kvp.Key);
                
                client.DefaultRequestHeaders.Add(kvp.Key, kvp.Value);
            }

            return client;
        }

        public static string ConstructUrlWithQueryString(string baseUrl, Dictionary<string, string> queryParameters)
        {
            var url = baseUrl;
            if (queryParameters != null)
            {
                url += ConstructQueryString(queryParameters);
            }

            return url;
        }
        
        public static string ConstructQueryString(Dictionary<string, string> parameters)
        {
            return "?" + string.Join("&", parameters.Select(kvp => kvp.Key + "=" + kvp.Value));
        }

        public static StringContent SerializeObjectToJsonStringContent<T>(T jsonContent)
        {
            var jsonString = SerializeObjectToJsonString(jsonContent);

            var content = new StringContent(jsonString);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Headers.ContentLength = jsonString.Length;

            return content;
        }
        
        public static string SerializeObjectToJsonString<T>(T jsonContent)
        {
            var isDataContract = 
                typeof(T).GetTypeInfo().GetCustomAttribute<DataContractAttribute>() != null;

            if (isDataContract)
            {
                var serializationStream = new MemoryStream();
                var serializer = new DataContractJsonSerializer(typeof(T));
                serializer.WriteObject(serializationStream, jsonContent);

                serializationStream.Position = 0;
                var sr = new StreamReader(serializationStream);
                var jsonString = sr.ReadToEnd();

                return jsonString;
            }
            else
            {
                var settings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All
                };

                var jsonString = JsonConvert.SerializeObject(jsonContent, settings);

                return jsonString;
            }
        }

        public static HttpContent SerializeObjectToXmlStringContent<T>(T xmlContent)
        {
            var serializationStream = new MemoryStream();
            var serializer = new DataContractSerializer(typeof(T));
            serializer.WriteObject(serializationStream, xmlContent);

            serializationStream.Position = 0;
            var sr = new StreamReader(serializationStream);
            var xml = sr.ReadToEnd();
            xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + xml;

            xml = FormatXml(xml);
            Console.WriteLine(xml);

            byte[] bytes = Encoding.ASCII.GetBytes(xml);
//            var utf8xml = Encoding.ASCII.GetString(bytes);
            var content = new ByteArrayContent(bytes);
            
            content.Headers.ContentType = new MediaTypeHeaderValue("text/xml");
//            content.Headers.ContentLength = memoryStream.Length;
            return content;
        }
        
        public static string SerializeObjectToXmlString<T>(T xmlContent)
        {
            var serializationStream = new MemoryStream();
            var serializer = new DataContractSerializer(typeof(T));
            serializer.WriteObject(serializationStream, xmlContent);

            serializationStream.Position = 0;
            var sr = new StreamReader(serializationStream);
            var xml = sr.ReadToEnd();
            xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" + xml;

            xml = FormatXml(xml);
            return xml;
        }
        
        private static string FormatXml(string sUnformattedXml)
        {
            XmlDocument xd = new XmlDocument();
            xd.LoadXml(sUnformattedXml);
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            XmlWriter xtw = null;
            try
            {
                xtw = XmlWriter.Create(sw, new XmlWriterSettings
                {
                    Indent = true
                });
                xd.WriteTo(xtw);
            }
            finally
            {
                //clean up even if error
                xtw?.Dispose();
            }

            return sb.ToString();
        }

        public static FormUrlEncodedContent ToFormUrlEncodedContent(this Dictionary<string, string> dictionary)
        {
            var content = new FormUrlEncodedContent(
                dictionary.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value)));

            return content;
        }

        public static void AddHeadersFromDictionary<T>(this T content, Dictionary<string, string> customHeaders) 
            where T : HttpContent
        {
            if (customHeaders == null) return;
            
            foreach (var kvp in customHeaders)
            {
                content.Headers.Add(kvp.Key, kvp.Value);
            }
        }
        
        public static async Task<string> SendFile(string url, string fileName, string requestXmlString)
        {
            const string BOUNDARY = "MIME_boundary";
            const string CRLF = "\r\n";
			
            //get HttpWebRequest object
            var req = (HttpWebRequest) WebRequest.Create(url);
            req.Method = "POST";

            //set http headers
            req.Headers["X-EBAY-API-COMPATIBILITY-LEVEL"] = "981";
            req.Headers["X-EBAY-API-SITEID"] = "0";
            req.Headers["X-EBAY-API-CALL-NAME"] = "UploadSiteHostedPictures";
            req.ContentType = "multipart/form-data; boundary=" + BOUNDARY;

            //read in the picture file
            var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            fs.Seek(0, SeekOrigin.Begin);
            var br = new BinaryReader(fs);
            var image = br.ReadBytes((int)fs.Length);
            br.Dispose();
            fs.Dispose();

            //first part of the post body
            var	strReq1= "--" + BOUNDARY + CRLF
               	         + "Content-Disposition: form-data; name=document" + CRLF
               	         + "Content-Type: text/xml; charset=\"UTF-8\"" + CRLF + CRLF
               	         + requestXmlString
               	         + CRLF + "--" + BOUNDARY + CRLF
               	         + "Content-Disposition: form-data; name=image; filename=image" + CRLF
               	         + "Content-Transfer-Encoding: binary" + CRLF
               	         + "Content-Type: application/octet-stream" + CRLF + CRLF;
			 
            //last part of the post body
            const string strReq2 = CRLF + "--" + BOUNDARY + "--" + CRLF;

            //Convert string to byte array
            var postDataBytes1 = System.Text.Encoding.ASCII.GetBytes(strReq1);
            var postDataBytes2 = System.Text.Encoding.ASCII.GetBytes(strReq2);

            var len = postDataBytes1.Length + postDataBytes2.Length + image.Length;
            req.Headers["Content-Length"] = len.ToString();

            //post the payload
            var requestStream = await req.GetRequestStreamAsync();
            requestStream.Write(postDataBytes1, 0, postDataBytes1.Length);
            requestStream.Write(image, 0, image.Length);
            requestStream.Write(postDataBytes2, 0, postDataBytes2.Length);
            requestStream.Dispose();

            //get response and write to console
            var resp =  (HttpWebResponse) await req.GetResponseAsync();
            var responseReader = new StreamReader(resp.GetResponseStream(), Encoding.UTF8);
            var response = responseReader.ReadToEnd();

            //log response message from eps server
            resp.Dispose();

            return response;
        }
        
        public class LoggingHandler : DelegatingHandler
        {
            private bool _enabled;
            public LoggingHandler(HttpMessageHandler innerHandler)
                : base(innerHandler)
            {
                _enabled = Environment.GetEnvironmentVariable("BRTHOR_HTTP_VERBOSE") != null;
            }

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (_enabled)
                {
                    Console.WriteLine("Request:");
                    Console.WriteLine(request.ToString());
                    if (request.Content != null)
                    {
                        var contentBytes = await request.Content.ReadAsByteArrayAsync();
                        Console.WriteLine(Encoding.ASCII.GetString(contentBytes));
                    }
                    Console.WriteLine();

                    HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

                    Console.WriteLine("Response:");
                    Console.WriteLine(response.ToString());
                    if (response.Content != null)
                    {
                        Console.WriteLine(await response.Content.ReadAsStringAsync());
                    }
                    Console.WriteLine();

                    return response;
                }
            }
            
            
        }
    }
}