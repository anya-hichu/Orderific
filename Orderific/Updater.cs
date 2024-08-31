using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;
using System;

namespace Orderific;
public class Updater : IDisposable
{
    private ICallGateSubscriber<int, string, object> SetCharacterTitle { get; init; }

    private Configuration Configuration { get; init; }
    private IFramework Framework { get; init; }
    private DateTime? LastTitleUpdateAt { get; set; }
    private int Index { get; set; } = 0;

    public Updater(Configuration configuration, IFramework framework, ICallGateSubscriber<int, string, object> setCharacterTitle)
    {
        Configuration = configuration;
        Framework = framework;
        SetCharacterTitle = setCharacterTitle;

        Framework.Update += OnFrameworkUpdate;
    }

    public void Dispose()
    {
        Framework.Update -= OnFrameworkUpdate;
        LastTitleUpdateAt = null;
        Index = 0;
    }

    public void OnFrameworkUpdate(IFramework framework)
    {
        var titleJsonLines = Configuration.TitleJsonLines();
        if (Configuration.Enabled && titleJsonLines.Length > 0 && Configuration.TitleJsonValid())
        {
            if (!LastTitleUpdateAt.HasValue || (DateTime.Now - LastTitleUpdateAt.Value).TotalSeconds > Configuration.Interval)
            {
                var lastIndexOrOverflow = Index >= titleJsonLines.Length;
                if (lastIndexOrOverflow)
                {
                    Index = 0;
                }

                SetCharacterTitle.InvokeAction(0, titleJsonLines[Index]);
                LastTitleUpdateAt = DateTime.Now;

                if(!lastIndexOrOverflow)
                {
                    Index++;
                }
            }
        }
    }
}
