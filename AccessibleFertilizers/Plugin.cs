using System;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Wish;

namespace AccessibleFertilizers
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        public static ManualLogSource logger;

        public static Sprite sprite;

        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
            logger = this.Logger;
            this.harmony.PatchAll();
            

            var spriteData =
                "iVBORw0KGgoAAAANSUhEUgAAAGQAAABkCAYAAABw4pVUAAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyZpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDkuMC1jMDAwIDc5LmRhNGE3ZTVlZiwgMjAyMi8xMS8yMi0xMzo1MDowNyAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIDI0LjEgKFdpbmRvd3MpIiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOjk5NDNFMTVFREIyQzExRUQ4MzQwQTA1RDcyQjY1QUJBIiB4bXBNTTpEb2N1bWVudElEPSJ4bXAuZGlkOjk5NDNFMTVGREIyQzExRUQ4MzQwQTA1RDcyQjY1QUJBIj4gPHhtcE1NOkRlcml2ZWRGcm9tIHN0UmVmOmluc3RhbmNlSUQ9InhtcC5paWQ6OTk0M0UxNUNEQjJDMTFFRDgzNDBBMDVENzJCNjVBQkEiIHN0UmVmOmRvY3VtZW50SUQ9InhtcC5kaWQ6OTk0M0UxNUREQjJDMTFFRDgzNDBBMDVENzJCNjVBQkEiLz4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz6QkaYOAAAJcUlEQVR42uxd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";
            
            var tex = new Texture2D(100, 100);;
            tex.LoadImage(System.Convert.FromBase64String(spriteData));
            sprite = Sprite.Create(tex, new Rect(0, 0, 100, 100), new Vector2(0.5f, 0.5f));
        }


        
        [HarmonyPatch(typeof(Crop))]
        class HarmonyPatch_Crop_Fertilize
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Crop), "Fertilize")]
            public static void Fertilize(ref Crop __instance, GameObject ____fertilized, FertilizerType fertilizerType, bool sendMeta = true)
            {
                try
                {
                    Transform transform = __instance.transform.Find("Modded Fertilizer");
                    if ((bool)transform)
                        Destroy(transform.gameObject);

                    if (fertilizerType == FertilizerType.None)
                    {
                        return;
                    }

                    var moddedFertilizer = new GameObject("Modded Fertilizer");
                    moddedFertilizer.transform.SetParent(__instance.transform);
                    var spriteRenderer = moddedFertilizer.AddComponent<SpriteRenderer>();
                    spriteRenderer.sprite = sprite;
                    spriteRenderer.drawMode = SpriteDrawMode.Simple;

                    if (fertilizerType is FertilizerType.Fire1 or FertilizerType.Fire2)
                    {
                        spriteRenderer.color = new Color(1, 0.04f, 0.04f, 0.75f);
                    }
                    else if (fertilizerType is FertilizerType.Water1 or FertilizerType.Water2 or FertilizerType.Sand)
                    {
                        spriteRenderer.color = new Color(0.2f, 1, 1, 0.35f);
                    }
                    else
                    {
                        spriteRenderer.color = new Color(0, 1, 0, 0.35f);
                    }

                    spriteRenderer.transform.SetParent(__instance.transform);
                    spriteRenderer.transform.eulerAngles = new Vector3(0f, 0.0f, 0.0f);
                    spriteRenderer.transform.localPosition = new Vector3(0.5f, 0.65f, -0.02f);
                    spriteRenderer.transform.localScale = new Vector3(0.95f, 1.2f, 1);

                }
                catch (Exception e)
                {
                    logger.LogError($"Plugin {PluginInfo.PLUGIN_GUID}: Error in Crop_Fertilize_Postfix: " + e);
                }
            }
        }
        
        
        [HarmonyPatch(typeof(Player), "Update")]
        class HarmonyPatch_Player_Update
        {
            private static bool PatchedPrefabs = false;
            
            private static bool Prefix(ref Player __instance)
            {
                if (!PatchedPrefabs)
                {
                    try
                    {
                        logger.LogInfo("Patching particles");

                        var particles = new[]
                        {
                            Prefabs.Instance.fertilizedParticlesEarth1,
                            Prefabs.Instance.fertilizedParticlesEarth2,
                            Prefabs.Instance.fertilizedParticlesFire1,
                            Prefabs.Instance.fertilizedParticlesFire2,
                            Prefabs.Instance.fertilizedParticlesWater1,
                            Prefabs.Instance.fertilizedParticlesWater2,
                        };
                    
                        foreach(var particle in particles)
                        {
                            particle.gravityModifier = 0.07f;
                            particle.emissionRate = 2f;
                            particle.playbackSpeed = 1;
                            particle.startSpeed = 0.2f;
                            particle.startSize = 0.5f;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"Plugin {PluginInfo.PLUGIN_GUID}: Error in Player_Update_Prefix: " + e);
                    }
                
                    PatchedPrefabs = true;
                
                }

                return true;
            }
        }
    }

}

