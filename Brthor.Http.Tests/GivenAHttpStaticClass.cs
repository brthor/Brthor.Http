using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using Brthor.Http;
using FluentAssertions;

namespace Brthor.Http.Tests
{
    public class GivenAHttpStaticClass
    {
        public class HttpBinResponse
        {
            public Dictionary<string, string> Headers { get; set; }
            
            public Dictionary<string, string> Args { get; set; }
            
            public dynamic Json { get; set; }
            
            public string Data { get; set; }
        }
        
        [Fact]
        public async Task ItCanGetJson()
        {
            var headersDictResponse = await Http.Get("https://httpbin.org/headers");
            var obj = headersDictResponse.Json<HttpBinResponse>();

            obj.Headers["Host"].Should().Be("httpbin.org");
        }

        [Fact]
        public void ItThrowsWhenGettingSiteWithSelfSignedCertificate()
        {
            // https://self-signed.badssl.com
            Action badAction = () =>
            {
                Http.Get("https://self-signed.badssl.com").GetAwaiter().GetResult(); 
            };

            badAction.ShouldThrow<HttpRequestException>();
        }
        
        [Fact]
        public async Task ItDoesNotThrowWhenGettingSiteWithSelfSignedCertificateAndVerifySslIsFalse()
        {
            await Http.Get("https://self-signed.badssl.com", verifySsl: false);
        }
        
        [Fact]
        public async Task ItCanSendCustomHeadersAndRequestDataWithAllRequestTypes()
        {
            var customHeaders = new Dictionary<string, string>
            {
                {"Test", "header"}
            };

            var queryParameters = new Dictionary<string, string>
            {
                {"Query", "param"}
            };

            var jsonContent = new {Test = "Json"};
            
            var responses = new[]
            {
                await Http.Get("https://httpbin.org/get", customHeaders: customHeaders, queryParameters: queryParameters),
                await Http.PostJson("https://httpbin.org/post", jsonContent, customHeaders: customHeaders, queryParameters: queryParameters),
                await Http.PutJson("https://httpbin.org/put", jsonContent, customHeaders: customHeaders, queryParameters: queryParameters),
                await Http.Delete("https://httpbin.org/delete", customHeaders: customHeaders, queryParameters: queryParameters)
            };

            foreach (var response in responses)
            {
                var obj = response.Json<HttpBinResponse>();
                obj.Headers["Test"].Should().Be("header");

                obj.Args["Query"].Should().Be("param");

                if (response.HttpResponseMessage.RequestMessage.Method == HttpMethod.Post
                    || response.HttpResponseMessage.RequestMessage.Method == HttpMethod.Put)
                {
                    string s = obj.Json.Test;
                    s.Should().Be("Json");
                    
                    Type t = obj.Json.GetType();
                    t.Should().Be(jsonContent.GetType());
                }
            }
        }
    }
}
