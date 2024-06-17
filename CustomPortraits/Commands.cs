using System;
using QFSW.QC;
using UnityEngine;
using Wish;

namespace CustomPortraits;

[CommandPrefix("/")]
public static class Commands
{
    [Command("npctalk")]
    public static void npctalk(string npc)
    {
        Plugin.logger.LogInfo(npc);

        foreach (var npcai in Resources.FindObjectsOfTypeAll<NPCAI>())
        {
            if (npcai.NPCName == npc)
            {
                npcai.Interact(0);
                return;
            }
        }

        throw new Exception("invalid NPC");
    }
}