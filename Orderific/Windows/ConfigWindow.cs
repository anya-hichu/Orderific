using System;
using System.IO;
using System.Numerics;
using Dalamud.Interface.ImGuiFileDialog;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Ipc;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace Orderific.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration { get; init; }
    private ICallGateSubscriber<int, string> GetCharacterTitle { get; init; }
    private ICallGateSubscriber<int, object> ClearCharacterTitle { get; init; }
    private FileDialogManager FileDialogManager { get; init; }

    public ConfigWindow(Configuration configuration, ICallGateSubscriber<int, string> getCharacterTitle, ICallGateSubscriber<int, object> clearCharacterTitle) : base("Orderific Config##configWindow")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(800, 280),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        Configuration = configuration;
        GetCharacterTitle = getCharacterTitle;
        ClearCharacterTitle = clearCharacterTitle;

        FileDialogManager = new FileDialogManager();
    }

    public void Dispose() { }

    public override void Draw()
    {
        var enabled = Configuration.Enabled;
        if (ImGui.Checkbox("Enable##enable", ref enabled))
        {
            Configuration.Enabled = enabled;
            Configuration.Save();

            if(!enabled)
            {
                ClearCharacterTitle.InvokeAction(0);
            }
        }

        var titleJsons = Configuration.TitleJsons;
        if (ImGui.InputTextMultiline("Title jsons (new line separated jsons)##titleJsons", ref titleJsons, 10000, new(ImGui.GetWindowWidth(), ImGui.GetWindowHeight() - ImGui.GetTextLineHeight() * 8))) {
            Configuration.TitleJsons = titleJsons;
            Configuration.Save();
        }

        if (!Configuration.TitleJsonValid())
        {
            ImGui.Text("Invalid Json Format");
        }


        if (ImGui.Button("Append current title##appendCurrentTitle"))
        {
            Configuration.TitleJsons += $"\n{GetCharacterTitle.InvokeFunc(0)}";
            Configuration.Save();
        }

        ImGui.SameLine();

        if (ImGui.Button("Import file content##importFileContent"))
        {
            FileDialogManager.OpenFileDialog("Import file content", ".*", (valid, paths) =>
            {
                if (valid)
                {
                    Configuration.TitleJsons = File.ReadAllText(paths[0]);
                    Configuration.Save();
                }
            }, 1);
        }




        var interval = Configuration.Interval;
        if (ImGui.InputInt("Interval in secs##interval", ref interval))
        {
            Configuration.Interval = interval;
            Configuration.Save();
        }

        FileDialogManager.Draw();
    }

}
