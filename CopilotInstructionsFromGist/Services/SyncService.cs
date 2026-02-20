using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

internal class SyncService
{
    public async Task<string> SyncAsync(string solutionDir, string gistUrl)
    {
        var gistId = ExtractGistId(gistUrl);

        var content = await DownloadGistAsync(gistId);

        var githubDir = Path.Combine(solutionDir, ".github");
        Directory.CreateDirectory(githubDir);

        var filePath = Path.Combine(githubDir, "copilot-instructions.md");

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, content);
            return "Copilot instructions created from Gist.";
        }

        var existingContent = File.ReadAllText(filePath);

        if (existingContent == content)
            return "Copilot instructions already up to date.";

        File.WriteAllText(filePath, content);
        return "Copilot instructions updated from Gist.";
    }

    private async Task<string> DownloadGistAsync(string gistId)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.UserAgent.ParseAdd("CopilotGistSync");

        var response = await http.GetAsync($"https://api.github.com/gists/{gistId}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var obj = JObject.Parse(json);

        return obj["files"]["copilot-instructions.md"]["content"]?.ToString();
    }

    private string ExtractGistId(string url)
    {
        var parts = url.TrimEnd('/').Split('/');
        return parts[parts.Length - 1];
    }
}
