using System;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DG.Tweening;
using HarmonyLib;
using Mirror;
using QFSW.QC;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wish;

namespace BetterMultiplayer;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
    public static ManualLogSource logger;
    private static Tween _showConsoleTween;
    private static bool _inPreviewMode;

    private static PlayerStatus _playerStatus;

    public static ConfigEntry<bool> ShowPlayerNames;
    public static ConfigEntry<string> PlayerStatusHotkey;

    private void Awake()
    {
        logger = Logger;
        this.harmony.PatchAll();
        
        ShowPlayerNames = Config.Bind<bool>("General", "Show Player Names", true, "Show player names above their character");
        PlayerStatusHotkey = Config.Bind<string>("General", "Player status hotkey", KeyCode.P.ToString(), "Unity Keycode to open the player status");
        
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
    }

    private static void EnableConsolePreviewMode()
    {
        QuantumConsole.Instance.gameObject.transform.Find("ConsoleRect/IOBar").gameObject.SetActive(false);
        QuantumConsole.Instance.gameObject.transform.Find("ConsoleRect/Console").GetComponent<Image>().color = new Color(1, 1, 1, 0.2f);
        ((RectTransform)QuantumConsole.Instance.transform.Find("ConsoleRect")).sizeDelta = new Vector2(280, 100);
        _inPreviewMode = true;
    }

    private static void DisableConsolePreviewMode()
    {
        QuantumConsole.Instance.gameObject.transform.Find("ConsoleRect/IOBar").gameObject.SetActive(true);
        QuantumConsole.Instance.gameObject.transform.Find("ConsoleRect/Console").GetComponent<Image>().color = new Color(1, 1, 1, 1);
        ((RectTransform)QuantumConsole.Instance.transform.Find("ConsoleRect")).sizeDelta = new Vector2(280, 160);
        _inPreviewMode = false;
        
        _showConsoleTween?.Kill();
    }
    
    [HarmonyPatch]
    private class Patches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(NetworkGamePlayer), "SetInitialData")]
        public static void SetInitialData(ref NetworkGamePlayer __instance)
        {
            try
            {
                if (__instance.player.transform.Find("Graphics/Layers/ChatBubble/Player name"))
                {
                    return;
                }

                var layers = __instance.player.transform.Find("Graphics/Layers");
                var cb = layers.Find("ChatBubble");
                var cbs = cb.GetComponent<ChatBubbleStack>();

                var playerName = Instantiate(Traverse.Create(cbs).Field("notificationPrefab").GetValue<ChatBubble>().transform, cb);
                playerName.name = "Player name";
                Destroy(playerName.Find("Image").GetComponent<Image>());
                playerName.GetComponent<Image>().color = new Color(0, 0, 0, 0);
                playerName.transform.localPosition = new Vector3(0, -13.15f, 0);

                var txt = playerName.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
                txt.color = Color.white;
                txt.outlineColor = Color.black;
                txt.fontSize = 0.45f;

                var component = txt.gameObject.AddComponent<PlayerName>();
                component.Initialize(__instance);
                
                playerName.gameObject.SetActive(true);
            }
            catch (Exception e)
            {
                logger.LogError(e);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(QuantumConsole), "InvokeCommand", typeof(string))]
        public static bool QCInvokeCommand(string command)
        {
            try
            {
                if (GameManager.Host && GameManager.Multiplayer && !command.IsNullOrWhiteSpace() && command.StartsWith("/kick "))
                {
                    var name = command.Remove(0, 6).Replace("\"", "");

                    foreach (var player in NetworkLobbyManager.Instance.GamePlayers)
                    {
                        if (player.Value.playerName.Equals(name) && player.Value.connectionToClient.connectionId != 0)
                        {
                            player.Value.connectionToClient.Disconnect();

                            DOVirtual.DelayedCall(0.1f, () => QuantumConsole.Instance.LogToConsole($"Kicked player {name}."));
                        }
                        else
                        {
                            DOVirtual.DelayedCall(0.1f, () => QuantumConsole.Instance.LogToConsole($"<color=red>No connected player with name {name} connected.</color>"));
                        }
                    }

                    return false;

                }

                return true;
            }
            catch (Exception e)
            {
                logger.LogError(e);
                return true;
            }
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Player), "SendChatMessage", typeof(string), typeof(string))]
        public static bool PlayerSendChatMessage(string characterName, string message)
        {
            try
            {
                QuantumConsole.Instance.LogPlayerText($"<{characterName}></color> {message}");
                return false;
            }
            catch (Exception e)
            {
                logger.LogError(e);
                return true;
            }
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(QuantumConsole), "LogPlayerText")]
        public static void QCLogPlayerText(ref string text)
        {
            try
            {
                var time = DateTime.Now.ToString("HH:mm");
                text = $"<color=green>[{time}]</color> {text}";

            }
            catch (Exception e)
            {
                logger.LogError(e);
            }
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuantumConsole), "LogPlayerText")]
        public static void QCLogPlayerText()
        {
            try
            {
                if (QuantumConsole.Instance.IsActive)
                {
                    return;
                }
            
                EnableConsolePreviewMode();
                QuantumConsole.Instance.Activate(false);

                _showConsoleTween?.Kill();
                _showConsoleTween = DOVirtual.DelayedCall(5f, () => QuantumConsole.Instance.Deactivate());
            }
            catch (Exception e)
            {
                logger.LogError(e);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(QuantumConsole), "Update")]
        public static void QCUpdate(ref QuantumConsole __instance, ref QuantumKeyConfig ____keyConfig)
        {
            try
            {
                if (_inPreviewMode && __instance.IsActive && ____keyConfig.ShowConsoleKey.IsPressed())
                {
                    DisableConsolePreviewMode();
                    QuantumConsole.Instance.FocusConsoleInput();
                }

                if (Time.frameCount % 10 != 0 || !__instance.IsActive) return;

                switch (_inPreviewMode)
                {
                    case true when !PlayerInput.AllowInput:
                        PlayerInput.EnableInput("console");
                        break;
                    case false when PlayerInput.AllowInput:
                        PlayerInput.DisableInput("console");
                        break;
                }
            }
            catch (Exception e)
            {
                logger.LogError(e);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(QuantumConsole), "Activate")]
        public static void QCActivate(ref bool shouldFocus)
        {
            try
            {
                if (shouldFocus)
                {
                    DisableConsolePreviewMode();
                }
            }
            catch (Exception e)
            {
                logger.LogError(e);
            }
        }
        
        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIHandler), "Initialize")]
        private static void UIHandlerInitialize(UIHandler __instance)
        {
            try
            {
                if (_playerStatus || !GameManager.Multiplayer)
                {
                    return;
                }
    
                _playerStatus = new GameObject("Player multiplayer status").AddComponent<PlayerStatus>();
                
                _playerStatus.transform.SetParent(__instance.transform);
                _playerStatus.transform.localPosition = Vector3.zero;
                _playerStatus.transform.localScale = Vector3.one;
                _playerStatus.transform.SetAsLastSibling();
                _playerStatus.gameObject.SetActive(true);
            }
            catch (Exception e)
            {
                logger.LogError(e);
            }
            
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIHandler), "UnloadGame")]
        private static void UIHandlerUnloadGame()
        {
            try
            {
                QuantumConsole.Instance.Deactivate();
            }
            catch (Exception e)
            {
                logger.LogError(e);
            }
        }
    }
}