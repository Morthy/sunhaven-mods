using System;
using System.Linq;
using HarmonyLib;
using UnityEngine.SceneManagement;
using Wish;

namespace WheresMyStuff;

[HarmonyPatch]
class Patches
{
    /*
    [HarmonyPostfix]
    [HarmonyPatch(typeof(UIHandler), "Initialize")]
    private static void UIHandlerInitialize(UIHandler __instance)
    {
        foreach (var x in SceneSettingsManager.Instance.sceneDictionary)
        {
            if (x.Value.playerMap)
            {
                Plugin.logger.LogInfo($"{x.Key} {x.Value.sceneName} {x.Value.formalSceneName}");                
            }
        }
    }
    */

    [HarmonyPatch(typeof(Chest), "SaveInventory")]
    class HarmonyPatch_Chest_SaveInventory
    {
        private static void Postfix(ref Chest __instance)
        {
            try
            {
                Plugin.ChestRepository.UpdateChestLocations(SceneManager.GetActiveScene().buildIndex, false);
                Plugin.ChestRepository.UpdateChestDirect(SceneManager.GetActiveScene().buildIndex, __instance);
            }
            catch (Exception e)
            {
                Plugin.logger.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(ItemData), nameof(ItemData.FormattedDescription), MethodType.Getter)]
    [HarmonyPostfix]
    public static void ItemData_FormattedDescription(ref ItemData __instance, ref string __result)
    {
        try
        {
            var itemId = __instance.id;
            var chestLocations = Plugin.ChestRepository.GetChestsWithItem(itemId);

            if (chestLocations.Count == 0)
            {
                __result += $"\n<color=red>0</color> in storage";
                return;
            }

            var count = chestLocations.Sum(chestLocation => chestLocation.GetCountOfItem(itemId));

            __result += $"\n<color=green>{count}</color> in storage";
            var resizedCount = 0;
            
            if (chestLocations.Count > 10)
            {
                resizedCount = chestLocations.Count - 10;
                chestLocations = chestLocations.GetRange(0, 10);
            }

            __result += "<line-height=38%><size=60%>\n";
            
            __result = chestLocations.Aggregate(__result, (current, chestLocation) => current + $"\n<color=green>{chestLocation.GetCountOfItem(itemId)}</color> in <i><color={chestLocation.GetHexColor()}>{chestLocation.friendlyName}</color></i> ({chestLocation.GetLocationName()})");

            if (resizedCount > 0)
            {
                __result += $"\n<size=60%>and more in {resizedCount} other chests! (you have a <b>lot</b> of chests)</size>";
            }

            __result += "</size></line-height>";
        }
        catch (Exception e)
        {
            Plugin.logger.LogError(e);
        }
    }
}