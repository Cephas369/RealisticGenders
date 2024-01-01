using System;
using System.Collections.Generic;
using System.Text;
using SandBox.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using FaceGen = TaleWorlds.Core.FaceGen;

namespace RealisticGenders
{
    internal class RGMissionBehavior : MissionBehavior
    {
        private static string[] _weaponsIds = { "peasant_hammer_1_t1", "peasant_hammer_2_t1", "practice_spear_t1", "western_spear_1_t2", "peasant_sickle_1_t1", "peasant_hatchet_1_t1", "peasant_polearm_1_t1", "peasant_pitchfork_2_t1", "throwing_stone" };

        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            base.OnAgentBuild(agent, banner);
            if (agent?.IsFemale == true)
            {
                agent.AgentDrivenProperties.CombatMaxSpeedMultiplier = (float)(agent.AgentDrivenProperties.CombatMaxSpeedMultiplier * 0.8);
                agent.AgentDrivenProperties.MaxSpeedMultiplier = (float)(agent.AgentDrivenProperties.MaxSpeedMultiplier * 0.8);


                agent.UpdateCustomDrivenProperties();
            }
            else if (RealisticGendersConfig.Instance.SpawnWomenAtSieges && Mission.IsSiegeBattle && agent?.Team?.Side == BattleSideEnum.Defender && !agent.IsHero)
            {
                if (MBRandom.RandomFloat > 0.65f)
                    SpawnRandomWoman();
            }
        }

        private void SpawnRandomWoman()
        {
            CharacterObject characterObject = Settlement.CurrentSettlement.Culture.VillageWoman;

            Monster monsterWithSuffix = FaceGen.GetMonsterWithSuffix(characterObject.Race, FaceGen.MonsterSuffixSettlement);

            Equipment randomEquipmentElements = Equipment.GetRandomEquipmentElements(characterObject, true, true);


            while (randomEquipmentElements[EquipmentIndex.Weapon0].IsEmpty)
            {
                randomEquipmentElements[EquipmentIndex.Weapon0] = new EquipmentElement(MBObjectManager.Instance.GetObjectTypeList<ItemObject>().Find(x => x.StringId == _weaponsIds[MBRandom.RandomInt(0, _weaponsIds.Length)]));
            }


            BodyProperties bodyProperties = FaceGen.GetRandomBodyProperties(characterObject.Race, true,
                characterObject.GetBodyPropertiesMin(), characterObject.GetBodyPropertiesMax(),
                (int)ArmorComponent.HairCoverTypes.All, MBRandom.RandomInt(0, 999999), null, null, null);

            AgentBuildData agentBuildData = new AgentBuildData(new BasicBattleAgentOrigin(characterObject)).Equipment(randomEquipmentElements).Monster(monsterWithSuffix).BodyProperties(bodyProperties).Team(Mission.Current.DefenderTeam).NoHorses(true).Formation(Mission.Current.DefenderTeam.GetFormation(FormationClass.Infantry));

            Agent female = Mission.Current.SpawnAgent(agentBuildData, true);
            female.SetWatchState(Agent.WatchState.Alarmed);
            female.SetMorale(10);
        }
    }
}
