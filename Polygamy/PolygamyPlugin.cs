using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Wish;

namespace Polygamy;

[BepInPlugin(pluginGuid, pluginName, pluginVersion)]
public class PolygamyPlugin : BaseUnityPlugin
{
    private const string pluginGuid = "vurawnica.sunhaven.polygamy";
    private const string pluginName = "Polygamy";
    private const string pluginVersion = "0.0.4";
    private Harmony m_harmony = new Harmony(pluginGuid);
    public static ManualLogSource logger;

    private void Awake()
    {
        // Plugin startup logic
        PolygamyPlugin.logger = this.Logger;
        logger.LogInfo((object)$"Plugin {pluginName} is loaded!");
        this.m_harmony.PatchAll();
    }

    [HarmonyPatch(typeof(NPCAI), "HandleWeddingRing")]
    class HarmonyPatch_NPCAI_HandleWeddingRing
    {
        private static string Postfix(string __result, out bool response, NPCAI __instance, ref string ____npcName)
        {
            response = false;
            bool flag = false;
            string text = "";
            float value;
            if (SingletonBehaviour<GameSave>.Instance.GetProgressBoolCharacter("MarriedTo" + ____npcName))
            {
                flag = true;
                switch (____npcName)
                {
                    case "Lynn":
                        text = "Hehe, you want to get married again?? That's cute sweetie, but I don't think it works that way.";
                        break;
                    case "Anne":
                        text = "I don't know if this is a little joke or not, but the gesture is really cute. And of course, I'd be happy to take a second ring!";
                        break;
                    case "Donovan":
                        text = "";
                        break;
                    case "Vaan":
                        text = "";
                        break;
                    case "Xyla":
                        text = "";
                        break;
                    case "Nathaniel":
                        text = "";
                        break;
                    case "Jun":
                        text = "";
                        break;
                    case "Liam":
                        text = "";
                        break;
                    case "Catherine":
                        text = "";
                        break;
                    case "Claude":
                        text = "";
                        break;
                    case "Kitty":
                        text = "";
                        break;
                    case "Wornhardt":
                        text = "";
                        break;
                    case "Darius":
                        text = "";
                        break;
                    case "Lucia":
                        text = "";
                        break;
                    case "Iris":
                        text = "";
                        break;
                    case "Vivi":
                        text = "";
                        break;
                    case "Shang":
                        text = "";
                        break;
                    case "Kai":
                        text = "";
                        break;
                    case "Miyeon":
                        text = "";
                        break;
                    case "Lucius":
                        text = "";
                        break;
                    case "Wesley":
                        text = "";
                        break;
                }
            }
            else if
                (
                !SingletonBehaviour<GameSave>.Instance.GetProgressBoolCharacter("Dating" + ____npcName) ||
                !SingletonBehaviour<GameSave>.Instance.GetProgressBoolCharacter(____npcName + " Cycle 14") ||
                !SingletonBehaviour<GameSave>.Instance.CurrentSave.characterData.Relationships.TryGetValue(____npcName, out value) ||
                value < 75f
                )
            {
                flag = true;
                switch (____npcName)
                {
                    case "Lynn":
                        text = "Oh - wow! I'm so sorry XX, but I don't think I'm quite ready for that. We should get to know each other better first, right?";
                        break;
                    case "Anne":
                        text = "Who do I look like to you? I'm a merchant, not a farm wife. I love the ring, but it's going to take more effort on your part before I say yes.";
                        break;
                    case "Donovan":
                        text = "";
                        break;
                    case "Vaan":
                        text = "";
                        break;
                    case "Xyla":
                        text = "";
                        break;
                    case "Nathaniel":
                        text = "";
                        break;
                    case "Jun":
                        text = "";
                        break;
                    case "Liam":
                        text = "";
                        break;
                    case "Catherine":
                        text = "";
                        break;
                    case "Claude":
                        text = "";
                        break;
                    case "Kitty":
                        text = "";
                        break;
                    case "Wornhardt":
                        text = "";
                        break;
                    case "Darius":
                        text = "";
                        break;
                    case "Lucia":
                        text = "";
                        break;
                    case "Iris":
                        text = "";
                        break;
                    case "Vivi":
                        text = "";
                        break;
                    case "Shang":
                        text = "";
                        break;
                    case "Kai":
                        text = "";
                        break;
                    case "Miyeon":
                        text = "";
                        break;
                    case "Lucius":
                        text = "";
                        break;
                    case "Wesley":
                        text = "";
                        break;
                }
            }
            if (flag)
            {
                return text + "[]<i>(You must be dating and not already married to this character, achieved 15 full hearts, and progressed far enough in the dialogue to marry them)</i>";
            }
            if (SingletonBehaviour<GameSave>.Instance.GetProgressBoolCharacter("Married"))
            {
                Player.Instance.Inventory.RemoveItem(6107, 1);
            }
            response = true;
            SingletonBehaviour<GameSave>.Instance.SetProgressBoolCharacter("EngagedToRNPC", value: true);
            switch (____npcName)
            {
                case "Lynn":
                    Player.Instance.QuestList.StartQuest("LynnMarriageQuest");
                    return "Oh! Heh, I really shouldn't be surprised. Actually, what's really surprising is... I don't think it's a bad idea. Sure, let's do it, XX![]We should do it at 4pm tomorrow at the event center! I'll take care of everything else, you just show up!";
                case "Anne":
                    Player.Instance.QuestList.StartQuest("AnneMarriageQuest");
                    return "Heh, it's about time my investment paid off. What do you mean \"what investment?\" I'm talking about <i>you</i>, XX![]Of course I'll marry you! I'll pay for someone to arrange the ceremony, you just meet me at the event square tomorrow at 4:00 pm.";
                case "Donovan":
                    Player.Instance.QuestList.StartQuest("DonovanMarriageQuest");
                    return "Ah, I figured this was coming... Well I won't hold you in suspense.[]Let's do it, XX - let's get married!![]I'll have someone set up a nice little ceremony in your Human town. Just show up at 4:00 pm and I'll take care of the rest!";
                case "Vaan":
                    Player.Instance.QuestList.StartQuest("VaanMarriageQuest");
                    return "Finally! Yes of course we should be married, XX! It makes all the sense in the world.[]We should do it in Sun Haven. I'll see about contacting your Archmage to set up the ceremony. You just make sure you're on time! Let's call it 4:00 pm. I'll see you tomorrow!";
                case "Xyla":
                    Player.Instance.QuestList.StartQuest("XylaMarriageQuest");
                    return "Oh! Heh, I really shouldn't be surprised. Actually, what's really surprising is... I don't think it's a bad idea. Sure, let's do it, XX![]I'll even set it up for you in your beloved Human town. Let's kick it off at 4:00 pm. Don't be late, sewer rat!";
                case "Nathaniel":
                    Player.Instance.QuestList.StartQuest("NathanielMarriageQuest");
                    return "Ah! Heh, it's about time![]Yes XX, I will absolutely marry you! Let me handle the ceremony, you just get yourself to the event square tomorrow at 4:00 pm. I can't wait!";
                case "Jun":
                    Player.Instance.QuestList.StartQuest("JunMarriageQuest");
                    return "XX! I've pictured this moment so many times... Yes, I will marry you![]Let's have the ceremony tomorrow at 4:00 pm. I'll get the event center all set up, you just need to be there. I'll see you then!";
                case "Liam":
                    Player.Instance.QuestList.StartQuest("LiamMarriageQuest");
                    return "Ah, wow, this is a lot.[]You know... I think I'm ready for this! Yes XX, let's get married!! I'll see about setting up a ceremony. You just meet me tomorrow at the event square. Let's say 4:00 pm!";
                case "Catherine":
                    Player.Instance.QuestList.StartQuest("CatherineMarriageQuest");
                    return "I've been dreaming of the moment! I thought I'd be prepared for it, but now I feel like I'm floating.[]Of course I'll marry you, XX!! I'll prepare the ceremony, you just meet me at the event square tomorrow at 4:00 pm.";
                case "Claude":
                    Player.Instance.QuestList.StartQuest("ClaudeMarriageQuest");
                    return "Marriage? With me? I never thought... Well, I guess it doesn't matter what I thought now.[]Yes, of course I will marry you, XX. I'll pay for somebody to set up the ceremony in the event square. Be there tomorrow at 4:00 pm. I really can't wait!";
                case "Kitty":
                    Player.Instance.QuestList.StartQuest("KittyMarriageQuest");
                    return "OH - oh my goodness gracious! XX, I will marry you! Kitty will marry XX, nya nya![]Don't worry, Kitty will get the ceremony set up. You just show up to the event square at 4:00 pm tomorrow! I can't wait!";
                case "Wornhardt":
                    Player.Instance.QuestList.StartQuest("WornhardtMarriageQuest");
                    return "... Really, do you mean this?[]XX, marrying you would make me the happiest man in town! Yes, let's do it! I'll handle the preparations, you just get yourself to the event square by 4:00 pm tomorrow.";
                case "Darius":
                    Player.Instance.QuestList.StartQuest("DariusMarriageQuest");
                    return "It's about time you asked, XX. I was growing impatient, but now you can take your proper place by the side of Withergate's future king.[]That is to say, I accept your proposal! I'll have some lackeys set up a ceremony tomorrow in your Human town. Be there at 4:00 pm, and don't keep me waiting.";
                case "Lucia":
                    Player.Instance.QuestList.StartQuest("LuciaMarriageQuest");
                    return "Oh my goodness!! XX!! Yes, yes <i>of course</i> I'll marry you![]I'll prepare the ceremony for us tomorrow. Try to be at the event square by 4:00 pm! I'm so excited, XX!";
                case "Iris":
                    Player.Instance.QuestList.StartQuest("IrisMarriageQuest");
                    return "You're proposing?? Oh - I'm sorry, I was just so unprepared for this.[]...The answer is yes, obviously! I know you'll want it in Sun Haven, so I'll talk to your Archmage to set it up. Just be there tomorrow at 4:00 pm. Ah, this is so exciting!";
                case "Vivi":
                    Player.Instance.QuestList.StartQuest("ViviMarriageQuest");
                    return "Oh YEAH? Well I'M happy that you're so impressive! You'll never have more passion than a Wildborn like me!!";
                case "Shang":
                    Player.Instance.QuestList.StartQuest("ShangMarriageQuest");
                    return "Why shouldn't I be? I feel powerful and alive. I have you, XX. I feel whole. You are the whole, XX.";
                case "Kai":
                    Player.Instance.QuestList.StartQuest("KaiMarriageQuest");
                    return "Then let us make a lifetime of memories together, XX. Ones that I can look back on and be happy with.";
                case "Miyeon":
                    Player.Instance.QuestList.StartQuest("MiyeonMarriageQuest");
                    return "What do you mean, XX? Are you talking about marriage? []...I could be ready for that with you. If you're ready, I mean. Are you ready?[] Ohh, no, don't tell me! I'm going to blush! I-is this really happening?!";
                case "Lucius":
                    Player.Instance.QuestList.StartQuest("LuciusMarriageQuest");
                    return "M-marriage?! Me? I suppose that WOULD make me even happier![] But could you ever imagine me getting married? I never did![] But maybe... there's a first time for everything? Oh, now I'm absolutely shaking with excitement!";
                case "Wesley":
                    Player.Instance.QuestList.StartQuest("WesleyMarriageQuest");
                    return "M-marriage?! Me? I suppose that WOULD make me even happier![] But could you ever imagine me getting married? I never did![] But maybe... there's a first time for everything? Oh, now I'm absolutely shaking with excitement!";

                default:
                    return "Oh! Heh, I really shouldn't be surprised. Actually, what's really surprising is... I don't think it's a bad idea. Sure, let's do it, XX![]We should do it at 4pm tomorrow at the event center! I'll take care of everything else, you just show up!";
            }
        }
    }
}