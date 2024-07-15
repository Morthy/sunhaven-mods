using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using PSS;
using TMPro;
using UnityEngine;
using Wish;

namespace RenameFridgesAndWardrobes;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private readonly Harmony _harmony = new(PluginInfo.PLUGIN_GUID);

    private void Awake()
    {
        _harmony.PatchAll();
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
    }
}