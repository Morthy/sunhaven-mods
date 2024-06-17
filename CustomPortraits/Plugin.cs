using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using DG.Tweening;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using Wish;

namespace CustomPortraits;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
    public static ManualLogSource logger;
    public static Plugin Instance;

    private static Dictionary<string, Texture2D> Portraits = new();
    private static Dictionary<string, Sprite> PortraitSpriteCache = new();
    
    private static int DebugEmoteIndex;
    private static bool IsCustomBust;

    public static ConfigEntry<bool> ModAuthor;

    private void Awake()
    {
        Instance = this;
        
        ModAuthor = Config.Bind<bool>("General", "ModAuthor", false, "If true, enables convenience shortcuts for mod authors");

        logger = this.Logger;
        harmony.PatchAll();
        LoadPortraits();
        Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
    }

    public void LoadPortraits()
    {
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "CustomPortraits");
        
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            return;
        }

        foreach (var file in Directory.GetFiles(path, "*.png", SearchOption.AllDirectories))
        {
            Portraits.Add(Path.GetFileNameWithoutExtension(file), LoadTexture(file));
        }
        
        logger.LogInfo($"Loaded {Portraits.Count} custom portraits");
    }

    private Texture2D LoadTexture(string path)
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.LoadImage(File.ReadAllBytes(path));
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.wrapModeU = TextureWrapMode.Clamp;
        tex.wrapModeV = TextureWrapMode.Clamp;
        tex.wrapModeW = TextureWrapMode.Clamp;
        return tex;
    }
    
    private static Sprite GetHDSprite(string npcName, string season, int index = 0)
    {
        var textureKey = npcName + "_" + season;
        var numSprites = season.Equals("Wedding") ? 1f : 6f;

        logger.LogInfo($"GetHDSprite {npcName} {season} {index}");
        
        if (Portraits.TryGetValue(textureKey, out Texture2D texture))
        {
            var cacheKey = textureKey + "_" + index;
            if (!PortraitSpriteCache.TryGetValue(cacheKey, out Sprite sprite))
            {
                sprite = Sprite.Create(texture, new Rect(index * texture.width / numSprites, 0, texture.width / numSprites, texture.height), new Vector2(0.5f, 0.5f), (float)Math.Round(texture.height / 211f * 24));
                sprite.name = $"Custom Portrait {npcName} {season} {index}";
                PortraitSpriteCache[cacheKey] = sprite;
            }

            return sprite;
        }

        return null;
    }

    [HarmonyPatch]
    class Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DialogueController), "LoadBust")]
        public static bool LoadBust(bool isMarriageBust, bool isSwimsuitBust, ref string ___npcName, ref Image ____bust)
        {
            try
            {
                Sprite ourSprite;
                
                if (isMarriageBust)
                {
                    ourSprite = GetHDSprite(___npcName, "Wedding");
                }
                else if (isSwimsuitBust)
                {
                    ourSprite = GetHDSprite(___npcName, "Swimsuit");
                }
                else
                {
                    ourSprite = GetHDSprite(___npcName, SingletonBehaviour<DayCycle>.Instance.Season.ToString());
                }

                if (ourSprite != null)
                {
                    ____bust.sprite = ourSprite; 
                    ____bust.SetNativeSize();
                    ____bust.gameObject.SetActive(true);
                    IsCustomBust = true;
                    return false;
                }

                IsCustomBust = false;

                return true;
            }
            catch (Exception e)
            {
                logger.LogError(e);
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DialogueController), "Emote", typeof(string), typeof(bool))]
        public static bool Emote(string emoteName, bool reset, ref string ___npcName, ref Image ____bust)
        {
            try
            {
                if (!IsCustomBust)
                {
                    return true;
                }

                DialogueController.Instance.StartCoroutine(DoEmote(emoteName.ToLower(), reset));

                return false;
            }
            catch (Exception e)
            {
                logger.LogError(e);
            }

            return true;
        }

        private static IEnumerator DoEmote(string emoteName, bool reset)
        {
            var npcName = Traverse.Create(DialogueController.Instance).Field("npcName").GetValue<string>();
            
            if (npcName.IsNullOrWhiteSpace())
            {
                yield break;
            }

            var bust = Traverse.Create(DialogueController.Instance).Field("_bust").GetValue<Image>();
            var originalSprite = bust.sprite;
            var tween = Traverse.Create(DialogueController.Instance).Field("emoteTween").GetValue<Tween>();

            yield return new WaitForSeconds(0.1f);

            var emoteIndex = emoteName switch
            {
                "romantic" => 1,
                "happy" => 2,
                "mad" => 3,
                "embarrassed" => 4,
                "sad" => 5,
                _ => 0
            };

            var sprite = GetHDSprite(npcName, SingletonBehaviour<DayCycle>.Instance.Season.ToString(), emoteIndex);
            bust.sprite = sprite;

            tween?.Kill();

            if (reset)
            {
                Traverse.Create(DialogueController.Instance).Field("emoteTween").SetValue(DOVirtual.DelayedCall(4.2f, () => { }, false).OnComplete(() => bust.sprite = originalSprite));
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "Update")]
        public static void PlayerUpdate(ref Player __instance)
        {
            if (!__instance.IsOwner || !ModAuthor.Value)
            {
                return;
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(KeyCode.Z) && DialogueController.Instance.DialogueOnGoing)
                {
                    if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.LeftShift))
                    {
                        DebugEmoteIndex = 0;
                    }
                    else {
                        DebugEmoteIndex++;

                        if (DebugEmoteIndex > 5)
                        {
                            DebugEmoteIndex = 0;
                        }
                    }
                    
                    var npcName = Traverse.Create(DialogueController.Instance).Field("npcName").GetValue<string>();

                    if (DebugEmoteIndex == 0)
                    {
                        DialogueController.Instance.SetDialogueBustVisualsOptimized(npcName, false, Input.GetKey(KeyCode.LeftAlt), Input.GetKey(KeyCode.LeftShift));
                    }
                    else
                    {
                        DialogueController.Instance.Emote((EmoteType)Enum.GetValues(typeof(EmoteType)).GetValue(DebugEmoteIndex), false);
                    }
                
                }

                if (Input.GetKeyDown(KeyCode.X))
                {
                    if (DialogueController.Instance.DialogueOnGoing)
                    {
                        NotificationStack.Instance.SendNotification("Cowardly refusing to reload while dialogue open");
                    }
                    else
                    {
                        Portraits.Clear();
                        PortraitSpriteCache.Clear();
                        Instance.LoadPortraits();
                    
                        MethodInfo dynMethod = typeof(DialogueController).GetMethod("Awake", 
                            BindingFlags.NonPublic | BindingFlags.Instance);
                        dynMethod.Invoke(DialogueController.Instance, new object[] {});

                        NotificationStack.Instance.SendNotification("Custom Portraits reloaded");
                    }
                }
            }
        }
    }
}