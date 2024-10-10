using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Wish;

namespace ScarecrowEnhancer;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
    public static ManualLogSource logger;

    public static Sprite sprite;


    private void Awake()
    {
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
        logger = this.Logger;
        this.harmony.PatchAll();
    }

    [HarmonyPatch]
    class HarmonyPatch_Decoration
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Decoration), "Awake")]
        private static void Awake(ref Decoration __instance)
        {
            if (__instance is not Scarecrow)
            {
                return;
            }
                
            if (!sprite)
            {
                logger.LogInfo("find sprite");
                sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(x => x.name.Equals("scarecrow_basic_placement"));
            }
                
            logger.LogInfo("add preview");
                
            __instance.gameObject.AddComponent<ScarecrowPreview>();
        }
    }
}