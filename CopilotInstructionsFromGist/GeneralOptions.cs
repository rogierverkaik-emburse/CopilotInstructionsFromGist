using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

public class GeneralOptions : DialogPage
{
    private string gistUrl = "";
    private bool enableAutoSync = false;

    [Category("General")]
    [DisplayName("Gist URL")]
    [Description("Public GitHub Gist URL containing copilot-instructions.md")]
    public string GistUrl
    {
        get => gistUrl;
        set => gistUrl = value;
    }

    [Category("General")]
    [DisplayName("Enable Auto Sync on Solution Open")]
    [Description("Automatically sync when solution opens.")]
    public bool EnableAutoSync
    {
        get => enableAutoSync;
        set => enableAutoSync = value;
    }
}
