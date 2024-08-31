using Dalamud.Configuration;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Orderific;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public bool Enabled { get; set; } = true;
    public int Interval { get; set; } = 60; // seconds
    public string TitleJsons { get; set; } = string.Empty; // New line separated title jsons

    public string[] TitleJsonLines()
    {
        return TitleJsons.Trim().Split("\n");
    }

    public bool TitleJsonValid()
    {
        try
        {
            foreach (var titleJsonLine in TitleJsonLines())
            {
                var _ = JsonDocument.Parse(titleJsonLine) != null;
            }
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    //CHECK IF VALID FORMAT + VALIDATE

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
