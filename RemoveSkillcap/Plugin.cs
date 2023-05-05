using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil;
using UnityEngine;
using Wish;

namespace RemoveSkillcap
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony = new(PluginInfo.PLUGIN_GUID);
        private static ManualLogSource logger;
        
        private const int ExpPerLevel = 2500;

        private void Awake()
        {            
            logger = Logger;
            harmony.PatchAll();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
        }

        [HarmonyPatch]
        class Patches
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Profession), "GetLevelFromExp")]
            public static bool ProfessionGetLevelFromExp(float exp, int[] ___expValues, ref int __result)
            {
                try
                {
                    if (exp >= ___expValues[___expValues.Length - 1])
                    {
                        var extra = exp - ___expValues[___expValues.Length - 1];
                        __result = ___expValues.Length + (int)Math.Floor(extra / ExpPerLevel);
                        return false;
                    }
                }
                catch (Exception e)
                {
                    logger.LogError($"Plugin {PluginInfo.PLUGIN_GUID}: Error in GetLevelFromExp " + e);
                }

                return true;
                
            }
            
            [HarmonyPrefix]
            [HarmonyPatch(typeof(Profession), "GetExpFromLevel")]
            public static bool ProfessionGetExpFromLevel(int level, int[] ___expValues, ref float __result)
            {
                try {
                    if (level > ___expValues.Length)
                    {
                        __result = ___expValues[___expValues.Length - 1] + (ExpPerLevel * (level - ___expValues.Length));
                        return false;
                    }
                }
                catch (Exception e)
                {
                    logger.LogError($"Plugin {PluginInfo.PLUGIN_GUID}: Error in GetExpFromLevel " + e);
                }

                return true;
            }
        }
        
    }

}

