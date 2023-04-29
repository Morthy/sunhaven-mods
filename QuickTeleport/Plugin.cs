using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using Wish;

namespace QuickTeleport
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        public static ManualLogSource logger;

        private static ConfigEntry<string> HotKey;

        private void Awake()
        {
            logger = this.Logger;
            try
            {
                Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
                HotKey = this.Config.Bind<string>("General", "Open menu hotkey", "F8", "Unity Keycode to open the menu");
                this.harmony.PatchAll();
            }
            catch (Exception e)
            {
                logger.LogError("{PluginInfo.PLUGIN_GUID} Awake failed: " + e);
            }
        }

        private static bool CanAccessNelvari()
        {
            return SingletonBehaviour<GameSave>.Instance.GetProgressBoolCharacter("NelvariFarm");
        }

        private static bool CanAccessWithergate()
        {
            return SingletonBehaviour<GameSave>.Instance.GetProgressBoolCharacter("Apartment");
        }

        private static bool CanAccessAltarRoom()
        {
            return SingletonBehaviour<GameSave>.Instance.GetProgressBoolCharacter("ConfrontingDynus4Quest");
        }

        public static DialogueNode GetDialogue() => new DialogueNode()
        {
            dialogueText = new List<string>()
            {
                "Teleport to where?"
            },
            responses = new Dictionary<int, Response>()
            {
                {
                    1,
                    new Response()
                    {
                        responseText = (StringFunction)(() => "Sunhaven"),
                        action = (UnityAction)(() =>
                        {
                            DialogueController.Instance.CancelDialogue(false);
                            DialogueController.Instance.PushDialogue(GetSunHavenDialogue());
                        })
                    }
                },
                {
                    2,
                    new Response()
                    {
                        responseText = (StringFunction)(() => CanAccessNelvari() ? "Nel'Vari" : "<color=red>Nel'Vari</color>"),
                        action = CanAccessNelvari() ? (UnityAction)(() => { SingletonBehaviour<ScenePortalManager>.Instance.ChangeScene(new Vector2(136, 100), "NelvariFarm"); }) : (UnityAction)null
                    }
                },
                {
                    3,
                    new Response()
                    {
                        responseText = (StringFunction)(() => CanAccessWithergate() ? "Withergate" : "<color=red>Withergate</color>"),
                        action = (UnityAction)(() =>
                        {
                            DialogueController.Instance.CancelDialogue(false);
                            DialogueController.Instance.PushDialogue(GetWithergateDialogue());
                        })
                    }
                },
                {
                    4,
                    new Response()
                    {
                        responseText = (StringFunction)(() => "Cancel"),
                        action = (UnityAction)null
                    }
                }
            }
        };

        public static DialogueNode GetSunHavenDialogue() => new DialogueNode()
        {
            dialogueText = new List<string>()
            {
                "Teleport to where in Sunhaven?"
            },
            responses = new Dictionary<int, Response>()
            {
                {
                    1,
                    new Response()
                    {
                        responseText = (StringFunction)(() => "Farm"),
                        action = (UnityAction)(() => { ScenePortalManager.Instance.ChangeScene(new Vector2(355.9167f, 124.1562f), "2playerfarm"); })
                    }
                },
                {
                    2,
                    new Response()
                    {
                        responseText = (StringFunction)(() => "Museum"),
                        action = (UnityAction)(() => { SingletonBehaviour<ScenePortalManager>.Instance.ChangeScene(new Vector2(130, 87.5f), "MuseumEntrance"); })
                    }
                },
                {
                    3,
                    new Response()
                    {
                        responseText = (StringFunction)(() => "Mines"),
                        action = (UnityAction)(() => { SingletonBehaviour<ScenePortalManager>.Instance.ChangeScene(new Vector2(89.82f, 105.8f), "Quarry"); })
                    }
                },
                {
                    4,
                    new Response()
                    {
                        responseText = (StringFunction)(() => "Cancel"),
                        action = (UnityAction)null
                    }
                }
            }
        };

        public static DialogueNode GetWithergateDialogue() => new DialogueNode()
        {
            dialogueText = new List<string>()
            {
                "Teleport to where in Withergate?"
            },
            responses = new Dictionary<int, Response>()
            {
                {
                    1,
                    new Response()
                    {
                        responseText = (StringFunction)(() => "Farm"),
                        action = (UnityAction)(() => { SingletonBehaviour<ScenePortalManager>.Instance.ChangeScene(new Vector2(138, 87.5f), "WithergateRooftopFarm"); })
                    }
                },
                {
                    2,
                    new Response()
                    {
                        responseText = (StringFunction)(() =>
                            CanAccessAltarRoom()
                                ? "Dynus Alter Room"
                                : "<color=red>Dynus Altar Room</color>"),
                        action = CanAccessAltarRoom()
                            ? (UnityAction)(() => { SingletonBehaviour<ScenePortalManager>.Instance.ChangeScene(new Vector2(199, 97), "DynusAltar"); })
                            : (UnityAction)null
                    }
                },
                {
                    3,
                    new Response()
                    {
                        responseText = (StringFunction)(() => "Cancel"),
                        action = (UnityAction)null
                    }
                }
            }
        };


        [HarmonyPatch(typeof(Player), "Update")]
        class HarmonyPatch_Player_Update
        {
            private static bool Prefix(ref Player __instance)
            {
                try
                {
                    if (!__instance.IsOwner)
                    {
                        return true;
                    }

                    if (!SingletonBehaviour<GameSave>.Instance.GetProgressBoolCharacter("Intro"))
                    {
                        return true;
                    }

                    if (!SingletonBehaviour<GameSave>.Instance.GetProgressBoolWorld("PlacedHouse"))
                    {
                        return true;
                    }


                    if (Input.GetKeyDown((KeyCode) System.Enum.Parse(typeof(KeyCode), HotKey.Value)) && !DialogueController.Instance.DialogueOnGoing)
                    {
                        if (Settings.EnableCheats && (HotKey.Value == "F8" || HotKey.Value == "F7"))
                        {
                            SingletonBehaviour<NotificationStack>.Instance.SendNotification("Cheats are enabled, which use F7/F8 to enable noclip/godmode.");
                            SingletonBehaviour<NotificationStack>.Instance.SendNotification("Please bind a different key for teleportation.");
                        }
                        else
                        {
                            DialogueController.Instance.SetDefaultBox();
                            DialogueController.Instance.PushDialogue(GetDialogue());
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.LogError("Error: " + e);
                }

                return true;
            }
        }
    }
}