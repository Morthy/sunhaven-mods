using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Wish;

namespace ScarecrowEnhancer
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
        }

        [HarmonyPatch(typeof(Scarecrow))]
        class HarmonyPatch_Scarecrow
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Scarecrow), "Update")]
            private static void Update(ref Scarecrow __instance)
            {
                if (!sprite)
                {
                    sprite = Resources.FindObjectsOfTypeAll<Sprite>().FirstOrDefault(x => x.name.Equals("scarecrow_basic_placement"));
                }

                var pos = __instance.RealCenter;
                pos.y -= pos.z;
                
                var distance = Vector2.Distance(Utilities.MousePositionExact(), pos);

                if (distance < 1 && !__instance.transform.Find("Scarecrow preview"))
                {
                    var preview = new GameObject("Scarecrow preview").AddComponent<SpriteRenderer>();
                    preview.sprite = sprite;
                    preview.sortingOrder = -1;
                    Transform transform1;
                    (transform1 = preview.transform).SetParent(__instance.transform);
                    transform1.localPosition = new Vector3(0, -4f, -4f);
                    transform1.localScale = new Vector3(1f, 1f, 1f);
                    transform1.eulerAngles = new Vector3(-45.0f, 0.0f, 0.0f);
                    preview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
                }
                else if (distance >= 1 && (__instance.transform.Find("Scarecrow preview")))
                {
                    Object.Destroy(__instance.transform.Find("Scarecrow preview").gameObject);
                }
            }
        }
    }

}

