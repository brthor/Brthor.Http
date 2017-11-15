# Brthor.Http: Easier Http Request API for .NET Standard

[![Build Status](https://travis-ci.org/brthor/Brthor.Http.svg?branch=master)](https://travis-ci.org/brthor/Brthor.Http)
[![Nuget Version Number](https://img.shields.io/nuget/v/Brthor.Http.svg)](https://www.nuget.org/packages/Brthor.Http)

## Examples


** Basic **
```c#

var response = Http.Get("www.google.com");
Console.WriteLine(response.Text);

```

** Send and receive json, send custom headers, and set query parameters. **
```
public class HttpBinJsonResponse
{
    public string Url;
}


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
    var responseJson = response.Json<HttpBinResponse>();
}
```