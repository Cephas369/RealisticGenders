using System;
using System.Diagnostics;
using System.Linq;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;
using TaleWorlds.SaveSystem.Load;

namespace RealisticGenders;

[HarmonyPatch(typeof(HeroCreator), "CreateSpecialHero")]
public static class AvoidQuestHeroChangesPatch
{
    public static bool ToAvoid = false;

    [HarmonyPrefix]
    public static void Prefix(CharacterObject template, Settlement bornSettlement = null, Clan faction = null,
        Clan supporterOfClan = null, int age = -1)
    {
        Type callerClass = new StackFrame(2).GetMethod().DeclaringType;
        if (typeof(QuestBase).IsAssignableFrom(callerClass) || typeof(IssueBase).IsAssignableFrom(callerClass))
        {
            ToAvoid = true;
        }
    }

    [HarmonyPostfix]
    public static void Postfix(CharacterObject template, Settlement bornSettlement = null, Clan faction = null,
        Clan supporterOfClan = null, int age = -1)
    {
        if (ToAvoid)
            ToAvoid = false;
    }
}