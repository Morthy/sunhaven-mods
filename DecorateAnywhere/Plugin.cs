using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Wish;

namespace DecorateAnywhere
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        public static ManualLogSource logger;

        private void Awake()
        {            
            logger = Logger;
            harmony.PatchAll();
            
            harmony.Patch(typeof(GameManager).GetMethods().First(
                    m => m.Name.Equals("InvalidDecorationPlacement") &&  m.GetParameters().Length == 8), 
                new HarmonyMethod(typeof(Patches).GetMethod("GameManagerInvalidDecorationPlacement"))
            );
            
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
        }

        [HarmonyPatch]
        class Patches
        {
            public static bool GameManagerInvalidDecorationPlacement(ref bool canBePlaced, ref Decoration decoration)
            {
                if (Player.Instance.pause || decoration is Crop)
                {
                    return true;
                }
                
                canBePlaced = true; // :)

                return false;
            }
        }
        
    }
}