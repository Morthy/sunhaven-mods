using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Wish;

namespace NPCConvo
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony = new Harmony("NPCConvo");
        public static ManualLogSource logger;
        
        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
            logger = this.Logger;
            this.harmony.PatchAll();
        }
        
        [HarmonyPatch(typeof(Relationships), "SetupRelationshipPanel")]
        class HarmonyPatch_Relationships_SetupRelationshipPanel
        {
            private static void Postfix(ref Relationships __instance)
            {
                Dictionary<string, RelationshipPanel> relationshipPanels =
                    Traverse.Create(__instance).Field("relationshipPanels").GetValue() as
                        Dictionary<string, RelationshipPanel>;

                foreach (KeyValuePair<string, float> relationship in SingletonBehaviour<GameSave>.Instance.CurrentSave
                             .characterData.Relationships)
                {
                    string key = relationship.Key;
                    NPCAI npc = SingletonBehaviour<NPCManager>.Instance.GetNPC(key);
                    
                    if ((bool)(Object)npc && npc.Romanceable)
                    {
                        int complete = 0;

                        for (int i = 1; i <= 30; i++)
                        {
                            if (!SingletonBehaviour<GameSave>.Instance.GetProgressBoolCharacter(key + " Cycle " + i))
                            {
                                complete = i - 1;
                                break;
                            }
                        }

                        if (complete > 14)
                        {
                            complete = 14;
                        }

                        if (relationshipPanels is not null && relationshipPanels.TryGetValue(key, out var relationshipPanel))
                        {
                            relationshipPanel.npcNameTMP.text = key + ", " + npc.title + " (" + complete + "/14)";
                        }
                    }

                }
            }
        }
    }
    
}
