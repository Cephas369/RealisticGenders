using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.TournamentGames;
using TaleWorlds.Core;

namespace RealisticGenders.Models
{
    internal class RGTournamentGame : FightTournamentGame
    {
        public RGTournamentGame(Town town) : base(town)
        {
        }
        public override bool CanBeAParticipant(CharacterObject character, bool considerSkills)
        {
            bool baseValue = base.CanBeAParticipant(character, considerSkills);
            if (character.IsFemale)
                return false;
            return baseValue;
        }
    }
    internal class RGTournamentModel : DefaultTournamentModel
    {
        public override TournamentGame CreateTournament(Town town)
        {
            return new RGTournamentGame(town);
        }
    }
}
