using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using static SyncService;
using Task = System.Threading.Tasks.Task;

namespace CopilotInstructionsFromGist;

[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
[ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
[ProvideOptionPage(typeof(GeneralOptions), "Copilot Gist Sync", "General", 0, 0, true)]
[Guid(PackageGuidString)]
[ProvideMenuResource("Menus.ctmenu", 1)]
public sealed class CopilotInstructionsFromGistPackage : AsyncPackage
{
    public const string PackageGuidString = "43f82a5f-e06b-4869-bee9-d5407b126afa";
    private EnvDTE.SolutionEvents _solutionEvents;

    protected override async Task InitializeAsync(
        CancellationToken cancellationToken,
        IProgress<ServiceProgressData> progress)
    {
        await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

        await SyncCommand.InitializeAsync(this);

        if (await GetServiceAsync(typeof(EnvDTE.DTE)) is EnvDTE.DTE dte)
        {
            _solutionEvents = dte.Events.SolutionEvents;
            _solutionEvents.Opened += () =>
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

        if (await GetServiceAsync(typeof(SVsStatusbar)) is not IVsStatusbar statusBar)
            return;

        uint cookie = 0;
        statusBar.Progress(ref cookie, 1, "Syncing Copilot instructions...", 0, 0);

        await TaskScheduler.Default;

        try
        {
            var syncService = new SyncService();
            var result = await syncService.SyncAsync(solutionDir, options.GistUrl);

            await JoinableTaskFactory.SwitchToMainThreadAsync();

            statusBar.Progress(ref cookie, 0, "", 0, 0);

            if (result.ResultType != SyncResultType.Unchanged)
            {
                statusBar.SetText(result.Message);

                await Task.Delay(5000);
                await JoinableTaskFactory.SwitchToMainThreadAsync();
                statusBar.SetText(string.Empty);
            }
        }
        catch
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            statusBar.Progress(ref cookie, 0, "", 0, 0);
            statusBar.SetText("Copilot Gist sync failed.");
        }
    }
}