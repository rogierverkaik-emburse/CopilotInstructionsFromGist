using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace CopilotInstructionsFromGist;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[ProvideOptionPage(typeof(GeneralOptions), "Copilot Gist Sync", "General", 0, 0, true)]
[Guid(CopilotInstructionsFromGistPackage.PackageGuidString)]
[ProvideMenuResource("Menus.ctmenu", 1)]
public sealed class CopilotInstructionsFromGistPackage : AsyncPackage
{
    public const string PackageGuidString = "43f82a5f-e06b-4869-bee9-d5407b126afa";

    protected override async Task InitializeAsync(
        CancellationToken cancellationToken,
        IProgress<ServiceProgressData> progress)
    {
        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        await SyncCommand.InitializeAsync(this);

        if (await GetServiceAsync(typeof(EnvDTE.DTE)) is EnvDTE.DTE dte)
        {
            dte.Events.SolutionEvents.Opened += () =>
            {
                _ = JoinableTaskFactory.RunAsync(async () =>
                {
                    await HandleSolutionOpenedAsync();
                });
            };
        }
    }
    private async Task HandleSolutionOpenedAsync()
    {
        await JoinableTaskFactory.SwitchToMainThreadAsync();

        var options = (GeneralOptions)GetDialogPage(typeof(GeneralOptions));

        if (!options.EnableAutoSync)
            return;

        if (string.IsNullOrWhiteSpace(options.GistUrl))
            return;

        var dte = await GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
        var solutionDir = Path.GetDirectoryName(dte.Solution.FullName);

        await TaskScheduler.Default;

        try
        {
            var syncService = new SyncService();
            await syncService.SyncAsync(solutionDir, options.GistUrl);
        }
        catch
        {
            // silent failure for auto-sync
        }
    }

}