using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Wish;

namespace QuickCastSpells
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
        class Patches
        {

            [HarmonyPrefix]
            [HarmonyPatch(typeof(PlayerInventory), "Update")]
            public static bool PlayerUpdatePrefix(ref PlayerInventory __instance)
            {
                try
                {
                    var player = Traverse.Create(__instance).Field("_player").GetValue<Player>();

                    if (!player || !player.IsOwner || !PlayerInput.AllowInput)
                    {
                        return true;
                    }

                    var playerTraverse = Traverse.Create(player);
                    var useItemNotUsing = playerTraverse.Field("useItemNotUsing").GetValue<bool>();
                    var spell1NotCasting = playerTraverse.Field("spell1NotCasting").GetValue<bool>();
                    var spell2NotCasting = playerTraverse.Field("spell2NotCasting").GetValue<bool>();
                    var spell3NotCasting = playerTraverse.Field("spell3NotCasting").GetValue<bool>();
                    var spell4NotCasting = playerTraverse.Field("spell4NotCasting").GetValue<bool>();

                    if (player && useItemNotUsing && spell2NotCasting && spell3NotCasting && spell4NotCasting)
                    {
                        if (PlayerInput.GetButtonDown(Button.Spell1) || MouseVisualManager.UsingController && (double)Input.GetAxis("DPadY") > 0.5)
                        {
                            player.Spell1?.UseDown1();
                            return false;
                        }

                        if (PlayerInput.GetButtonUp(Button.Spell1))
                        {
                            player.Spell1?.UseUp1();
                            return false;
                        }

                        if (PlayerInput.GetButton(Button.Spell1))
                        {
                            player.Spell1?.Use1();
                            return false;
                        }
                    }

                    if (player && useItemNotUsing && spell1NotCasting && spell3NotCasting && spell4NotCasting)
                    {
                        if (PlayerInput.GetButtonDown(Button.Spell2) || MouseVisualManager.UsingController && (double)Input.GetAxis("DPadX") > 0.5)
                        {
                            player.Spell2?.UseDown1();
                            return false;
                        }

                        if (PlayerInput.GetButtonUp(Button.Spell2))
                        {
                            player.Spell2?.UseUp1();
                            return false;
                        }

                        if (PlayerInput.GetButton(Button.Spell2))
                        {
                            player.Spell2?.Use1();
                            return false;
                        }

                    }

                    if (player && useItemNotUsing && spell1NotCasting && spell2NotCasting && spell4NotCasting)
                    {
                        if (PlayerInput.GetButtonDown(Button.Spell3) || MouseVisualManager.UsingController && (double)Input.GetAxis("DPadY") < -0.5)
                        {
                            player.Spell3?.UseDown1();
                            return false;
                        }

                        if (PlayerInput.GetButtonUp(Button.Spell3))
                        {
                            player.Spell3?.UseUp1();
                            return false;
                        }

                        if (PlayerInput.GetButton(Button.Spell3))
                        {
                            player.Spell3?.Use1();
                            return false;
                        }

                    }

                    if (player && player.Spell4 && useItemNotUsing && spell1NotCasting && spell2NotCasting && spell3NotCasting)
                    {
                        if (PlayerInput.GetButtonDown(Button.Spell4) || MouseVisualManager.UsingController && (double)Input.GetAxis("DPadX") < -0.5)
                        {
                            player.Spell4?.UseDown1();
                            return false;
                        }

                        if (PlayerInput.GetButtonUp(Button.Spell4))
                        {
                            player.Spell4?.UseUp1();
                            return false;
                        }

                        if (PlayerInput.GetButton(Button.Spell4))
                        {
                            player.Spell4?.Use1();
                            return false;
                        }
                    }

                    if (PlayerInput.GetButton(Button.Spell1) || PlayerInput.GetButton(Button.Spell2) || PlayerInput.GetButton(Button.Spell3) || PlayerInput.GetButton(Button.Spell4))
                    {
                        return false;
                    }

                    /*if (PlayerInput.GetButton(Button.Emote1))
                    {
                        Player.Instance.Mana = 500;
                    }*/

                    return true;
                }
                catch (Exception e)
                {
                    logger.LogInfo(e);
                }

                return true;
            }
        }
    }
}