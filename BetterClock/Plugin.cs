using System;
using BepInEx;
using HarmonyLib;
using TMPro;
using UnityEngine.UI;
using Wish;

namespace BetterClock;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private Harmony harmony = new(PluginInfo.PLUGIN_GUID);

    private void Awake()
    {
        harmony.PatchAll();
    }

    [HarmonyPatch]
    class Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(DayCycle), "UpdateTimeText")]
        public static void DayCycleUpdateTimeText(ref TextMeshProUGUI ____timeTMP)
        {
            var text = DayCycle.Instance.Time.ToString("HH:") + (DayCycle.Instance.Time.Minute / 10 * 10).ToString("00");

            if (DayCycle.Instance.Time.Hour >= 22 || DayCycle.Instance.Time.Hour <= 0)
            {
                text = $"<color=red>{text}</color>";
            }

            ____timeTMP.text = text;
        }
    }
}