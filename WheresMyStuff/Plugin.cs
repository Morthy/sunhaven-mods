using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using PSS;
using QFSW.QC;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Wish;
using Object = UnityEngine.Object;

namespace WheresMyStuff;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
    public static ManualLogSource logger;
    public static ChestRepository ChestRepository = new();

    private void Awake()
    {
        logger = Logger;
        this.harmony.PatchAll();
        ScenePortalManager.onLoadedScene += delegate
        {
            try
            {
                ChestRepository.UpdateChestLocations();
            }
            catch (Exception e)
            {
                logger.LogError(e);
            }
        };
        
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
    }
}