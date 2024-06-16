using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using PSS;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Wish;
using AnimationClip = Wish.AnimationClip;

namespace CustomTextures
{
    [BepInPlugin("aedenthorn.Morthy.CustomTextures", "Custom Textures", "2.0.0")]
    public partial class BepInExPlugin : BaseUnityPlugin
    {
        private static BepInExPlugin context;

        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> isDebug;
        public static ConfigEntry<KeyboardShortcut> reloadKey;

        public static Dictionary<string, string> customTextureDict = new Dictionary<string, string>();
        public static Dictionary<string, Texture2D> cachedTextureDict = new Dictionary<string, Texture2D>();
        public static Dictionary<string, Sprite> cachedSprites = new Dictionary<string, Sprite>();
        public static void Dbgl(string str = "", bool pref = true)
        {
            if (isDebug.Value)
                context.Logger.LogInfo(str);
        }
        private void Awake()
        {

            context = this;
            modEnabled = Config.Bind<bool>("General", "Enabled", true, "Enable this mod");
            isDebug = Config.Bind<bool>("General", "IsDebug", true, "Enable debug logs");
            reloadKey = Config.Bind<KeyboardShortcut>("General", "ReloadKey", new KeyboardShortcut(KeyCode.F5), "Key to press to reload textures from disk");

            var harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), Info.Metadata.GUID);

            LoadCustomTextures();

            foreach(var t in typeof(ClothingLayerData).GetTypeInfo().GetNestedTypes(BindingFlags.NonPublic | BindingFlags.Instance))
            {
                foreach(var m in t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (m.Name.Contains("LoadClothingSprites"))
                    {
                        Dbgl($"Found method {t.Name}:{m.Name}");
                        harmony.Patch(
                            original: m,
                            transpiler: new HarmonyMethod(typeof(BepInExPlugin), nameof(BepInExPlugin.ClothingLayerData_LoadClothingSprites_Transpiler))
                        );
                    }
                }
            }

            //SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            ScenePortalManager.onFinishLoadingDecorations += SceneManager_sceneLoaded;
            Database.OnDataFinishedLoading += ItemLoaded;

        }

        private static void ItemLoaded(int itemID)
        {
            if (!modEnabled.Value)
            {
                return;
            }
            //Dbgl($"Loaded {itemID}");
            
            Database.GetData<ItemData>(itemID, (item) =>
            {
                item.icon = TryGetReplacementSprite(item.icon);

                if (!item.useItem || !(item.useItem is Placeable placeable)) return;


                if (placeable._previewSprite)
                {
                    placeable._previewSprite = TryGetReplacementSprite(placeable._previewSprite);
                }

                if (placeable._secondaryPreviewSprite)
                {
                    placeable._secondaryPreviewSprite = TryGetReplacementSprite(placeable._secondaryPreviewSprite);
                }

                if (!placeable._decoration) return;

                foreach (var childGenerator in placeable._decoration.GetComponentsInChildren<MeshGenerator>())
                {
                    if (childGenerator && childGenerator.sprite)
                    {
                        childGenerator.sprite = TryGetReplacementSprite(childGenerator.sprite);
                    }
                }
            });
        }

        [HarmonyPatch(typeof(Player), "Update")]
        static class Player_Update_Patch
        {
            static void Postfix()
            {
                if (!modEnabled.Value)
                    return;

                if (reloadKey.Value.IsDown())
                {
                    cachedTextureDict.Clear();
                    LoadCustomTextures();
                }
            }
        }
        
        private static void LoadCustomTextures()
        {
            customTextureDict.Clear();
            string path = AedenthornUtils.GetAssetPath(context, true);
            foreach(string file in Directory.GetFiles(path, "*.png", SearchOption.AllDirectories))
            {
                customTextureDict.Add(Path.GetFileNameWithoutExtension(file), file);
            }
            Dbgl($"Loaded {customTextureDict.Count} textures");
            if(DialogueController.Instance != null)
            {
                DialogueController.Instance.enabled = false;
                DialogueController.Instance.enabled = true;
            }
        }
        private static Texture2D GetTexture(string path)
        {
            if (cachedTextureDict.TryGetValue(path, out var tex))
                return tex;
            TextureCreationFlags flags = new TextureCreationFlags();
            tex = new Texture2D(1, 1, GraphicsFormat.R8G8B8A8_UNorm, flags);
            tex.LoadImage(File.ReadAllBytes(path));
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.wrapModeU = TextureWrapMode.Clamp;
            tex.wrapModeV = TextureWrapMode.Clamp;
            tex.wrapModeW = TextureWrapMode.Clamp;
            cachedTextureDict[path] = tex;
            return tex;
        }
        public static IEnumerable<CodeInstruction> ClothingLayerData_LoadClothingSprites_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Dbgl("transpiling ClothingLayerData.LoadClothingSprites");

            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Call && codes[i].operand is MethodInfo && (MethodInfo)codes[i].operand == AccessTools.PropertyGetter(typeof(AsyncOperationHandle<ClothingLayerSprites>), nameof(AsyncOperationHandle<ClothingLayerSprites>.Result)))
                {
                    Dbgl("Found getter");
                    codes.Insert(i + 1, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BepInExPlugin), nameof(BepInExPlugin.HandleResult))));
                    i++;
                }
            }
            return codes.AsEnumerable();
        }

        private static void SceneManager_sceneLoaded()
        {
            if (!modEnabled.Value)
                return;
            Stopwatch s = new Stopwatch();
            Dbgl($"Replacing scene textures");
            s.Start();
            foreach (var c in FindObjectsOfType<Component>())
            {
                if(c is Renderer)
                {
                    foreach(var m in (c as Renderer).materials)
                    {
                        foreach(var n in m.GetTexturePropertyNames())
                        if(m.HasProperty(n) && m.GetTexture(n) is Texture2D)
                        {
                            m.SetTexture(n, TryGetReplacementTexture((Texture2D)m.GetTexture(n)));
                        }
                    }
                }
                else if (c is MonoBehaviour)
                {
                    FindSpritesInObject(c);
                }
            }
            Dbgl($"Time to replace textures: {s.ElapsedMilliseconds}ms");
        }

        private static void FindSpritesInObject(object c)
        {
            foreach (var f in AccessTools.GetDeclaredFields(c.GetType()))
            {
                var fo = f.GetValue(c);
                 if (f.FieldType == typeof(Sprite))
                {
                    //Dbgl($"found Sprite {c.GetType().Name} {f.Name}");

                    f.SetValue(c, TryGetReplacementSprite((Sprite)fo));
                }
                else if (f.FieldType == typeof(List<Sprite>))
                {
                    var field = (List<Sprite>)fo;
                    if (field is null)
                        continue;
                    //Dbgl($"found List<Sprite> {c.GetType().Name} {f.Name}");
                    for (int i = 0; i < field.Count; i++)
                    {
                        field[i] = TryGetReplacementSprite(field[i]);
                    }
                }
                else if (f.FieldType == typeof(Sprite[]))
                {
                    var field = (Sprite[])fo;
                    if (field is null)
                        continue;
                    //Dbgl($"found Sprite[] {c.GetType().Name} {f.Name}");
                    for (int i = 0; i < field.Length; i++)
                    {
                        field[i] = TryGetReplacementSprite(field[i]);
                    }
                }
                else if (f.FieldType == typeof(Dictionary<string, Sprite>))
                {
                    var field = (Dictionary<string, Sprite>)fo;
                    if (field is null)
                        continue;
                    //Dbgl($"found Sprite[] {c.GetType().Name} {f.Name}");
                    foreach(var k in field.Keys.ToArray())
                    {
                        field[k] = TryGetReplacementSprite(field[k]);
                    }
                }
                else if (f.FieldType == typeof(Dictionary<string, List<Sprite>>))
                {
                    var field = (Dictionary<string, List<Sprite>>)fo;
                    if (field is null)
                        continue;
                    //Dbgl($"found Sprite[] {c.GetType().Name} {f.Name}");
                    foreach(var k in field.Keys.ToArray())
                    {
                        for (int i = 0; i < field[k].Count; i++)
                        {
                            field[k][i] = TryGetReplacementSprite(field[k][i]);
                        }
                    }
                }
                else if(fo != null && !(fo is Enum) && !(fo is MonoBehaviour) && f.FieldType.Namespace == "Wish")
                {
                    //Dbgl($"checking field {c.GetType().Name} {f.Name}");

                    FindSpritesInObject(fo);
                }
            }
        }

        private static ClothingLayerSprites HandleResult(ClothingLayerSprites result)
        {
            if (result?._clothingLayerInfo == null)
            {
                return result;
            }
            for (int i = 0; i < result._clothingLayerInfo.Count; i++)
            {
                if (result._clothingLayerInfo[i].sprites == null)
                    continue;
                for (int j = 0; j < result._clothingLayerInfo[i].sprites.Length; j++)
                {
                    var textureName = result._clothingLayerInfo[i].sprites[j]?.texture?.name;
                    if (textureName is null)
                        continue;
                    result._clothingLayerInfo[i].sprites[j] = TryGetReplacementSprite(result._clothingLayerInfo[i].sprites[j]);
                }
            }
            return result;
        }

        private static Sprite TryGetReplacementSprite(Sprite oldSprite)
        {
            if (oldSprite == null)
                return null;
            var textureName = oldSprite.texture?.name;
            if (textureName == null)
                return oldSprite;

            if (cachedSprites.TryGetValue(oldSprite.name + "_" + textureName, out Sprite newSprite))
                return newSprite;
            if (!customTextureDict.TryGetValue(textureName, out string path))
                return oldSprite;

            //Dbgl($"replacing sprite {oldSprite.texture.name}");
            var newTex = GetTexture(path);
            newTex.name = oldSprite.texture.name;
            newSprite = Sprite.Create(newTex, oldSprite.rect, new Vector2(oldSprite.pivot.x / oldSprite.rect.width, oldSprite.pivot.y / oldSprite.rect.height), oldSprite.pixelsPerUnit, 0, SpriteMeshType.FullRect, oldSprite.border, true);
            newSprite.name = oldSprite.name;
            cachedSprites.Add(newSprite.name + "_" + textureName, newSprite);
            return newSprite;
        }
        private static Texture2D TryGetReplacementTexture(Texture2D oldTexture)
        {
            var textureName = oldTexture?.name;
            if (textureName == null || !customTextureDict.TryGetValue(textureName, out string path))
                return oldTexture;
            
            var newTex = GetTexture(path);
            newTex.name = oldTexture.name;
            return newTex;
        }
        [HarmonyPatch(typeof(AnimationHandler), "Awake")]
        static class AnimationHandler_Awake_Patch
        {
            static void Postfix(AnimationHandler __instance, Dictionary<string, AnimationClip> ____animationClips)
            {
                if (!modEnabled.Value)
                    return;

                foreach (var key in ____animationClips.Keys.ToArray())
                {
                    for(int i = 0; i < ____animationClips[key].Frames.Count; i++)
                    {
                        ____animationClips[key].Frames[i] = TryGetReplacementSprite(____animationClips[key].Frames[i]);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MeshGenerator), nameof(MeshGenerator.GenerateMesh))]
        static class MeshGenerator_GenerateMesh_Patch
        {
            static void Prefix(MeshGenerator __instance, Sprite ____prevSprite)
            {
                if (!modEnabled.Value || __instance.sprite == ____prevSprite)
                    return;

                __instance.sprite = TryGetReplacementSprite(__instance.sprite);
            }
        }
        
        [HarmonyPatch(typeof(MeshGenerator), nameof(MeshGenerator.ChangeSprite))]
        static class MeshGenerator_ChangeSprite_Patch
        {
            static void Prefix(MeshGenerator __instance)
            {
                if (!modEnabled.Value)
                    return;

                __instance.sprite = TryGetReplacementSprite(__instance.sprite);
            }
        }
    }
}
