using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using Wish;

namespace BanishSnaccoons
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        public static ManualLogSource logger;

        private void Awake()
        {
            logger = this.Logger;
            try
            {
                
                Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
                this.harmony.PatchAll();
            }
            catch (Exception e)
            {
                logger.LogError("{PluginInfo.PLUGIN_GUID} Awake failed: " + e);
            }
        }
        
        [HarmonyPatch]
        class HarmonyPatch_HungryMonster
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(HungryMonster), "CheckIfMonsterFull")]
            private static bool PrefixCheckIfMonsterFull(ref HungryMonster __instance, ref bool __result)
            {
                logger.LogInfo("PrefixCheckIfMonsterFull");
                
                try
                {
                    if (__instance.bundleType == BundleType.Snaccoon)
                    {
                        __result = true;
                        return false;
                    }
                }
                catch (Exception e)
                {
                    logger.LogError($"Plugin {PluginInfo.PLUGIN_GUID} HungryMonster_CheckIfMonsterFull error: {e}");
                }

                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(HungryMonster), nameof(HungryMonster.Interact))]
            public static bool PrefixInteract(ref HungryMonster __instance)
            {
                logger.LogInfo("PrefixInteract");
                
                try
                {
                    if (__instance.bundleType == BundleType.Snaccoon)
                    {
                        __instance.UpdateFullness(true);
                        AudioManager.Instance.PlayAudioImmediate(SingletonBehaviour<Prefabs>.Instance.snaccoonCrunch, 0.35f);
                        return false;
                    }
                }
                catch (Exception e)
                {
                    logger.LogError($"Plugin {PluginInfo.PLUGIN_GUID} HungryMonster_Interact error: {e}");
                }

                return true;
            }
        }
    }
}