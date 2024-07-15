using HarmonyLib;
using PSS;
using TMPro;
using UnityEngine;
using Wish;

namespace RenameFridgesAndWardrobes;

[HarmonyPatch]
internal class Patches
{
    private static GameObject _renamerPrefab;
    
    [HarmonyPatch(typeof(MainMenuController), "Start")]
    class HarmonyPatchMainMenuControllerStart
    {
        private static void Postfix()
        {
            Database.GetData<ItemData>(ItemID.Chest, chest =>
            {
                var chestUi = ((Chest)((Placeable)chest.useItem)._decoration).ui;
                _renamerPrefab = chestUi.transform.Find("ExternalInventory/Panel/ExternalInventoryTitle/InputField (TMP)").gameObject;
            });
        }
    }

    [HarmonyPatch(typeof(Chest), "Awake")]
    class HarmonyPatchChestAwake
    {
        private static void Postfix(ref Chest __instance, ref SunHavenInputField ___chestName, ref GameObject ___ui)
        {
            if (__instance.GetType() != typeof(Chest))
            {
                return;
            }

            if (___chestName)
            {
                return;
            }

            var ui = ___ui;
            var ourChest = __instance;

            if (ui.transform.Find("ExternalInventory/Panel/ExternalInventoryTitle/InputField (TMP)"))
            {
                return;
            }
                
            var ourNameChanger = Object.Instantiate(_renamerPrefab, ui.transform.Find("ExternalInventory/Panel/ExternalInventoryTitle"));
            ui.transform.Find("ExternalInventory/Panel/ExternalInventoryTitle/Text (TMP)").gameObject.SetActive(false);
            ourChest.chestName = ourNameChanger.GetComponent<SunHavenInputField>();
            ourChest.chestName.onEndEdit = new SunHavenInputField.SubmitEvent();
            ourChest.chestName.onEndEdit.AddListener(delegate(string newName)
            {
                ourChest.SetChestNameAndUpdate(newName);
            });
        }
    }
}