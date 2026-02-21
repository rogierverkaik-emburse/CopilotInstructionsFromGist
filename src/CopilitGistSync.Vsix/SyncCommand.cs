using CopilotGistSync.Core;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Threading.Tasks;

namespace CopilotGistSync.Vsix;

internal sealed class SyncCommand
{
    public const int CommandId = 0x0100;

    public static readonly Guid CommandSet = new("00ff5f52-4a27-454b-b263-523fec23ad38");

    private readonly AsyncPackage _package;

    private readonly ISyncService _syncService;

    private SyncCommand(AsyncPackage package, OleMenuCommandService commandService, ISyncService syncService)
    {
        _package = package ?? throw new ArgumentNullException(nameof(package));
        commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
        _syncService = syncService ?? throw new ArgumentNullException(nameof(syncService));

        var menuCommandID = new CommandID(CommandSet, CommandId);
        var menuItem = new MenuCommand(this.Execute, menuCommandID);
        commandService.AddCommand(menuItem);
    }

    public static SyncCommand Instance
    {
        get;
        private set;
    }

    public static async Task InitializeAsync(AsyncPackage package, ISyncService syncService)
    {
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        if (await package.GetServiceAsync(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
        {
            _ = new SyncCommand(package, commandService, syncService);
        }
    }

    private async void Execute(object sender, EventArgs e)
    {
        try
        {
            await ExecuteAsync();
        }
        catch (Exception ex)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            VsShellUtilities.ShowMessageBox(
                this._package,
                ex.Message,
                "Error",
                OLEMSGICON.OLEMSGICON_CRITICAL,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }

    private async Task ExecuteAsync()
    {
        // STEP 1: UI thread only for VS services
        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        var dte = await _package.GetServiceAsync(typeof(EnvDTE.DTE)) as EnvDTE.DTE;

        if (dte?.Solution == null || !dte.Solution.IsOpen)
        {
            ShowMessage("No solution is open.");
            return;
        }

        var options = (GeneralOptions)_package.GetDialogPage(typeof(GeneralOptions));
        var gistUrl = options.GistUrl;

        if (string.IsNullOrWhiteSpace(gistUrl))
        {
            ShowMessage("Please configure the Gist URL in Tools → Options.");
            return;
        }

        var solutionDir = Path.GetDirectoryName(dte.Solution.FullName);

        // STEP 2: Switch to background thread
        await TaskScheduler.Default;

        var resultMessage = await _syncService.SyncAsync(solutionDir, gistUrl);

        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

        ShowMessage(resultMessage.Message);
    }

    private void ShowMessage(string message)
    {
        ThreadHelper.ThrowIfNotOnUIThread();

        VsShellUtilities.ShowMessageBox(
            this._package,
            message,
            "Copilot Gist Sync",
            OLEMSGICON.OLEMSGICON_INFO,
            OLEMSGBUTTON.OLEMSGBUTTON_OK,
            OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
    }
}