using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Wish;
using ZeroFormatter;

namespace SaveBackups
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        public static ManualLogSource logger;
        private static ConfigEntry<int> RetentionDays;

        private void Awake()
        {            
            logger = Logger;
            this.harmony.PatchAll();
            RetentionDays = this.Config.Bind<int>("General", "Days kept", 30, "Number of in-game days for which backups should be kept");
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} v{PluginInfo.PLUGIN_VERSION} is loaded!");
        }

        [HarmonyPatch]
        class Patches
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(GameSave), "WriteCharacterToFile")]
            public static void WriteCharacterToFile(ref GameSave __instance, bool backup, bool newCharacter, string ___characterFolder)
            {
                try
                {

                    var fileExtension = typeof(GameSave).GetField("fileExtension", BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null);
                    
                    if (!backup || fileExtension == null ||  newCharacter)
                    {
                        return;
                    }

                    // Write the backup save file
                    GameSaveData gameSaveData = (GameSaveData)__instance.GetType().GetMethod("CopySaveData", BindingFlags.NonPublic | BindingFlags.Instance)
                        .Invoke(__instance, new object[] { __instance.CurrentSave });

                    int gameSaveDay = DayCycle.DayFromTime(gameSaveData.worldData.time);
                    
                    // if the player passed out, take away one day to prevent conflicts of backup files even though this save is technically for the next day
                    if (gameSaveData.worldData.time.Hour < 6)
                    {
                        gameSaveDay -= 1;
                    }

                    string str = gameSaveData.characterData.characterIndex == 0 ? "" : gameSaveData.characterData.characterIndex.ToString();
                    var dirPath = Application.persistentDataPath + "/" + ___characterFolder + "/Backups/" + Regex.Replace(gameSaveData.characterData.characterName, "<|>|=|#", "") + str;

                    if (!Directory.Exists(dirPath))
                    {
                        Directory.CreateDirectory(dirPath);
                    }

                    var path = dirPath + "/day" + gameSaveDay + "." + fileExtension;
                    if (File.Exists(path))
                    {
                        return;
                    }

                    logger.LogDebug("Writing backup to: " + path);
                    byte[] bytes = GameSave.CompressBytes(ZeroFormatterSerializer.Serialize(gameSaveData));
                    File.WriteAllBytes(path, bytes);
                    
                    // Cleanup old saves
                    foreach (string file in Directory.GetFiles(dirPath, "day*." + fileExtension, SearchOption.TopDirectoryOnly))
                    {
                        Match m = Regex.Match(file, @"day(\d+)\.");

                        if (m.Success)
                        {
                            int backupDay = Int32.Parse(m.Groups[1].Value);

                            if (backupDay != 0)
                            {
                                int daysOld = gameSaveDay - backupDay;
                                logger.LogDebug($"Found backup from day {backupDay} with path {file}, it's {daysOld} days old");

                                if (daysOld > RetentionDays.Value)
                                {
                                    logger.LogDebug($"Deleting {file} due to being too old");
                                    File.Delete(file);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.LogError($"Plugin {PluginInfo.PLUGIN_GUID} error in WriteCharacterToFile: {e}");
                }
            }
        }
    }
}

