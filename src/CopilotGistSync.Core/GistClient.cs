using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace CopilotGistSync.Core;

public interface IGistClient
{
    Task<string> GetFileContentAsync(string gistUrl, string fileName);
}

public class DefaultGistClient : IGistClient
{
    private static readonly HttpClient _http = CreateClient();

    private static HttpClient CreateClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd("CopilotGistSync");
        client.Timeout = TimeSpan.FromSeconds(10);
        return client;
    }

    public async Task<string> GetFileContentAsync(string gistUrl, string fileName)
    {
        var gistId = ExtractGistId(gistUrl);

        var response = await _http.GetAsync($"https://api.github.com/gists/{gistId}");

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var obj = JObject.Parse(json);

        var content = obj["files"]?[fileName]?["content"]?.ToString();

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException($"File '{fileName}' not found in Gist.");
        }

        return content;
    }

    private static string ExtractGistId(string url)
    {
        var uri = new Uri(url);
        return uri.Segments.Last().TrimEnd('/');
    }
}