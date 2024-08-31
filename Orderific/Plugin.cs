using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Orderific.Windows;
using Dalamud.Plugin.Ipc;

namespace Orderific;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;

    private const string CommandName = "/orderific";
    private const string CommandHelpMessage = $"Available subcommands for {CommandName} are config, disable and enable";

    private ICallGateSubscriber<int, object> ClearCharacterTitle { get; init; }
   
    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Orderific");
    private ConfigWindow ConfigWindow { get; init; }

    private Updater Updater { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        var getCharacterTitle = PluginInterface.GetIpcSubscriber<int, string>("Honorific.GetCharacterTitle");
        ClearCharacterTitle = PluginInterface.GetIpcSubscriber<int, object>("Honorific.ClearCharacterTitle");
        ConfigWindow = new ConfigWindow(Configuration, getCharacterTitle, ClearCharacterTitle);

        var setCharacterTitle = PluginInterface.GetIpcSubscriber<int, string, object>("Honorific.SetCharacterTitle");
        Updater = new(Configuration, Framework, setCharacterTitle);

        WindowSystem.AddWindow(ConfigWindow);
        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = CommandHelpMessage
        });

        PluginInterface.UiBuilder.Draw += DrawUI;

        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        PluginInterface.UiBuilder.OpenMainUi += ToggleConfigUI;
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        var subcommand = args.Split(" ", 2)[0];

        if (subcommand == "config")
        {
            ToggleConfigUI();
        }
        else if (subcommand == "enable")
        {
            Configuration.Enabled = true;
            Configuration.Save();
        }
        else if (subcommand == "disable")
        {
            ClearCharacterTitle.InvokeAction(0);
            Configuration.Enabled = false;
            Configuration.Save();
        }
        else
        {
            ChatGui.Print(CommandHelpMessage);
        }
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
}
