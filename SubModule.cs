using System.Collections.Generic;
using System.Text.RegularExpressions;
using HarmonyLib;
using NetworkMessages.FromServer;
using RealisticGenders.Models;
using SandBox.Missions.MissionLogics;
using SandBox.View.Map;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.SaveSystem;
using TaleWorlds.ScreenSystem;

namespace RealisticGenders
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            Harmony harmony = new Harmony("RealisticGenders");
            harmony.PatchAll();
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);

            if ((!mission.IsFieldBattle && !mission.IsSiegeBattle) || mission == null ||
                !mission.HasMissionBehavior<BattleAgentLogic>())
                return;

            mission.RemoveMissionBehavior(mission.GetMissionBehavior<BattleAgentLogic>());
            mission.AddMissionBehavior(new RGBattleAgentLogic());

            mission.AddMissionBehavior(new RGMissionBehavior());
        }

        protected override void OnGameStart(Game game, IGameStarter StarterObject)
        {
            if (game.GameType is Campaign)
            {
                CampaignGameStarter gameStarterObject = (CampaignGameStarter)StarterObject;


                gameStarterObject.AddBehavior(new RealisticGendersBehavior());

                gameStarterObject.AddModel(new RGTournamentModel());
                gameStarterObject.AddModel(new RGVolunteerModel());
                gameStarterObject.AddModel(new RGApplyDamageModel());

            }
        }
    }

    internal class RGBattleAgentLogic : BattleAgentLogic
    {
        public override void OnScoreHit(Agent affectedAgent, Agent affectorAgent, WeaponComponentData attackerWeapon, bool isBlocked,
            bool isSiegeEngineHit, in Blow blow, in AttackCollisionData collisionData, float damagedHp, float hitDistance,
            float shotDifficulty)
        {
            if (affectorAgent.IsFemale && affectorAgent.Origin.BattleCombatant is null)
                return;
            base.OnScoreHit(affectedAgent, affectorAgent, attackerWeapon, isBlocked, isSiegeEngineHit, in blow, in collisionData, damagedHp, hitDistance, shotDifficulty);

        }
    }

    public class RealisticGendersTypeDefiner : SaveableTypeDefiner
    {
        public RealisticGendersTypeDefiner() : base(234656153) { }
        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(List<Occupation>));
            ConstructContainerDefinition(typeof(Occupation[]));
        }
        
        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(RGTournamentGame), 0);
        }
    }
}
