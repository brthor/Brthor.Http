using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Brthor.Http
{
    public static class Http
    {
        public static async Task<HttpResponse> Get(string baseUrl, 
            Dictionary<string, string> queryParameters=null,
            Dictionary<string, string> customHeaders=null)
        {
            var client = HttpUtilities.ConstructHttpClientWithHeaders(customHeaders);
            var url = HttpUtilities.ConstructUrlWithQueryString(baseUrl, queryParameters);
            
            var response = await client.GetAsync(url);
            var responseContent = await response.Content.ReadAsStreamAsync();
            
            return new HttpResponse(response, responseContent);
        }
        
        public static async Task<HttpResponse> PutJson<T>(string baseUrl, 
            T jsonContent=null,
            Dictionary<string, string> queryParameters=null,
            Dictionary<string, string> customHeaders=null,
            Dictionary<string, string> customContentHeaders=null) where T : class
        {
            var content = HttpUtilities.SerializeObjectToJsonStreamContent(jsonContent);
            content.AddHeadersFromDictionary(customContentHeaders);

            return await Put(baseUrl, content, queryParameters: queryParameters, customHeaders: customHeaders);
        }
        
        public static async Task<HttpResponse> PutXml<T>(string baseUrl, 
            T xmlContent=null,
            Dictionary<string, string> queryParameters=null,
            Dictionary<string, string> customHeaders=null,
            Dictionary<string, string> customContentHeaders=null) where T : class
        {
            var content = HttpUtilities.SerializeObjectToXmlStringContent(xmlContent);
            content.AddHeadersFromDictionary(customContentHeaders);

            return await Put(baseUrl, content, queryParameters: queryParameters, customHeaders: customHeaders);
        }
        
        public static async Task<HttpResponse> Put(string baseUrl, 
            HttpContent content,
            Dictionary<string, string> queryParameters=null,
            Dictionary<string, string> customHeaders=null)
        {
            var client = HttpUtilities.ConstructHttpClientWithHeaders(customHeaders);
            var url = HttpUtilities.ConstructUrlWithQueryString(baseUrl, queryParameters);
            
            var response = await client.PutAsync(url, content);
            var responseContent = await response.Content.ReadAsStreamAsync();
            
            return new HttpResponse(response, responseContent);
        }
        
        public static async Task<HttpResponse> PostJson<T>(string baseUrl, 
            T jsonContent=null,
            Dictionary<string, string> queryParameters=null,
            Dictionary<string, string> customHeaders=null,
            Dictionary<string, string> customContentHeaders=null) where T : class
        {
            var content = HttpUtilities.SerializeObjectToJsonStreamContent(jsonContent);
            content.AddHeadersFromDictionary(customContentHeaders);

            return await Post(baseUrl, content, 
                queryParameters: queryParameters, 
                customHeaders: customHeaders);
        }
        
        public static async Task<HttpResponse> PostXml<T>(string baseUrl, 
            T xmlContent=null,
            Dictionary<string, string> queryParameters=null,
            Dictionary<string, string> customHeaders=null,
            Dictionary<string, string> customContentHeaders=null) where T : class
        {
            var content = HttpUtilities.SerializeObjectToXmlStringContent(xmlContent);
            content.AddHeadersFromDictionary(customContentHeaders);

            return await Post(baseUrl, content, 
                queryParameters: queryParameters, 
                customHeaders: customHeaders);
        }
        
        public static async Task<HttpResponse> Post(string baseUrl, 
            HttpContent content,
            Dictionary<string, string> queryParameters=null,
            Dictionary<string, string> customHeaders=null)
        {
            var client = HttpUtilities.ConstructHttpClientWithHeaders(customHeaders);
            var url = HttpUtilities.ConstructUrlWithQueryString(baseUrl, queryParameters);
            
            var response = await client.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStreamAsync();
            
            return new HttpResponse(response, responseContent);
        }
        
        public static async Task<HttpResponse> Delete(string baseUrl, 
            Dictionary<string, string> queryParameters=null,
            Dictionary<string, string> customHeaders=null)
        {
            var client = HttpUtilities.ConstructHttpClientWithHeaders(customHeaders);
            var url = HttpUtilities.ConstructUrlWithQueryString(baseUrl, queryParameters);
            
            var response = await client.DeleteAsync(url);
            var responseContent = await response.Content.ReadAsStreamAsync();
            
            return new HttpResponse(response, responseContent);
        }
    }
}