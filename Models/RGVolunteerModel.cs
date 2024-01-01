using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;

namespace RealisticGenders.Models;

internal class RGVolunteerModel : DefaultVolunteerModel
{
    public override float GetDailyVolunteerProductionProbability(Hero hero, int index, Settlement settlement)
    {
        return base.GetDailyVolunteerProductionProbability(hero, index, settlement) * 2;
    }

    public override int MaximumIndexHeroCanRecruitFromHero(Hero buyerHero, Hero sellerHero, int useValueAsRelation = -101)
    {
        int baseValue = base.MaximumIndexHeroCanRecruitFromHero(buyerHero, sellerHero, useValueAsRelation);
        if (!buyerHero.IsFemale)
            return baseValue;
        int overrideValue = baseValue * 2;
        return overrideValue > sellerHero.VolunteerTypes.Length ? sellerHero.VolunteerTypes.Length : overrideValue;

    }
}