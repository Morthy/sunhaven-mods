using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DG.Tweening;
using HarmonyLib;
using PSS;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wish;

namespace FarmScreenshots;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
    public static ManualLogSource logger;
    public static ScreenshotMaker ScreenshotMaker;
    private static ConfigEntry<string> HotKey;


    private void Awake()
    {
        logger = Logger;
        HotKey = this.Config.Bind<string>("General", "Screenshot Hotkey", "F11", "Unity Keycode to open the menu");
        this.harmony.PatchAll();
        
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
    }

    [HarmonyPatch]
    private class Patches
    { 
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Wish.Player), "Update")]
        public static void PlayerUpdatePostfix(ref Wish.Player __instance)
        {
            if (!__instance.IsOwner || Cutscene.Active || __instance.Pathing || UIHandler.InventoryOpen || __instance.pause || __instance.Sleeping)
            {
                return;
            }


            if (!(Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown((KeyCode) Enum.Parse(typeof(KeyCode), HotKey.Value))))
            {
                return;
            }

            if (ScreenshotMaker != null)
            {
                return;
            }

            logger.LogInfo(SceneManager.GetActiveScene().name);

            if (SceneManager.GetActiveScene().name.Equals("2playerfarm"))
            {
                var smObject = new GameObject("ScreenshotMaker");
                ScreenshotMaker = smObject.AddComponent<ScreenshotMaker>();
                ScreenshotMaker.cameraPosition = new Vector3(331.9659f, 60.1794f, -50f);
                ScreenshotMaker.orthographicSize = 45f;
                ScreenshotMaker.aspectRatio = 1f;
                ScreenshotMaker.screenshotWidth = 5000;
                ScreenshotMaker.screenshotHeight = 5000;
            }
            else if (SceneManager.GetActiveScene().name.Equals("NelvariFarm"))
            {
                var smObject = new GameObject("ScreenshotMaker");
                ScreenshotMaker = smObject.AddComponent<ScreenshotMaker>();
                ScreenshotMaker.cameraPosition = new Vector3(107.5376f, 33.8658f,-50f);
                ScreenshotMaker.orthographicSize = 22f;
                ScreenshotMaker.aspectRatio = 1.8f;
                ScreenshotMaker.screenshotWidth = 3000;
                ScreenshotMaker.screenshotHeight = 1666;
            }
            else if (SceneManager.GetActiveScene().name.Equals("WithergateRooftopFarm"))
            {
                var smObject = new GameObject("ScreenshotMaker");
                ScreenshotMaker = smObject.AddComponent<ScreenshotMaker>();
                ScreenshotMaker.cameraPosition = new Vector3(140.7435f, 17.7507f,-50f);
                ScreenshotMaker.orthographicSize = 24f;
                ScreenshotMaker.aspectRatio = 1.8f;
                ScreenshotMaker.screenshotWidth = 3000;
                ScreenshotMaker.screenshotHeight = 1666;
            }            
            else
            {
                NotificationStack.Instance.SendNotification("Screenshots are not supported in this location.");
            }

            if (!ScreenshotMaker) return;
            
            
            NotificationStack.Instance.SendNotification("Taking screenshot...");
            DOVirtual.DelayedCall(1f, () => ScreenshotMaker.TakeScreenshot());
        }
    }
}