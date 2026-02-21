using System.IO;
using System.Threading.Tasks;

namespace CopilotGistSync.Core;

public interface ISyncService
{
    Task<SyncResult> SyncAsync(string solutionDir, string gistUrl);
}

public enum SyncResultType
{
    Created,
    Updated,
    Unchanged
}

public class SyncResult
{
    public SyncResultType ResultType { get; set; }
    public string Message { get; set; }
}

public class SyncService(IGistClient gistClient, IFileSystem fileSystem) : ISyncService
{
    public async Task<SyncResult> SyncAsync(string solutionDir, string gistUrl)
    {
        var content = await gistClient.GetFileContentAsync(gistUrl, "copilot-instructions.md");

        var githubDir = Path.Combine(solutionDir, ".github");
        fileSystem.CreateDirectory(githubDir);

        var filePath = Path.Combine(githubDir, "copilot-instructions.md");

        if (!fileSystem.FileExists(filePath))
        {
            fileSystem.WriteAllText(filePath, content);
            return new SyncResult
            {
                ResultType = SyncResultType.Created,
                Message = "Copilot instructions created from Gist."
            };
        }

        var existingContent = fileSystem.ReadAllText(filePath);

        if (existingContent == content)
        {
            return new SyncResult
            {
                ResultType = SyncResultType.Unchanged,
                Message = "Copilot instructions already up to date."
            };
        }

        fileSystem.WriteAllText(filePath, content);

        return new SyncResult
        {
            ResultType = SyncResultType.Updated,
            Message = "Copilot instructions updated from Gist."
        };
    }
}
