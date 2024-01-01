using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using Extensions = TaleWorlds.Library.Extensions;

namespace RealisticGenders
{
    public class RealisticGendersBehavior : CampaignBehaviorBase
    {
        private bool UpdatedHeroGenders = false;
        private List<Occupation> _onlyMaleOccupations;
        //Old value still in use because if removed saves that used it will crash
        private Occupation[] onlyMaleOccupations;
        
        public static RealisticGendersBehavior? Instance;
        private readonly List<Hero> womenFolk = new();

        public static bool CanFemaleLordLead(Hero lady)
        {
            if (!lady.IsPregnant && RealisticGendersConfig.Instance?.DisableFemaleCondition == false)
            {
                bool rulerOverride = true;
                Clan ladyClan = lady.Clan;
                Kingdom ladyKingdom = ladyClan?.Kingdom;
                if (rulerOverride && ladyKingdom != null && ladyKingdom.Leader == lady)
                {
                    return true;
                }

                CultureCode leaderCulture = lady.Culture.GetCultureCode();
                Hero father = lady.Father;
                IEnumerable<Hero> siblings = lady.Siblings;
                Hero spouse = lady.Spouse;
                bool haveFather = father != null && !father.IsDead;
                bool haveSpouse = spouse != null && !spouse.IsDead;
                bool siblingsOK = !siblings.Any((Hero x) => x.IsAlive && x.Age >= 16f && !x.IsFemale);
                if (!haveFather && siblingsOK && !haveSpouse)
                {
                    return true;
                }
            }

            if (RealisticGendersConfig.Instance?.AllowRulersToLead == true && lady.IsFactionLeader)
                return true;

            return false;
        }

        public RealisticGendersBehavior()
        {
            Instance = this;
        }

        private bool isNewGame = false;

        private bool IsFemaleAndActiveParty(MobileParty party) => party?.IsMainParty == false && party.IsActive &&
                                                                  party.IsLordParty &&
                                                                  party.LeaderHero?.IsFemale == true;

        public override void RegisterEvents()
        {
            CampaignEvents.OnAfterSessionLaunchedEvent.AddNonSerializedListener(this, AfterSessionLaunched);
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, campaignGame => { isNewGame = true; });
            CampaignEvents.TickEvent.AddNonSerializedListener(this, OnTick);
            CampaignEvents.HeroCreated.AddNonSerializedListener(this, OnHeroCreated);
            CampaignEvents.CanHeroLeadPartyEvent.AddNonSerializedListener(this, CanHeroLeadParty);
        }

        private void OnTick(float f)
        {
            if (isNewGame)
            {
                ShowInquiry();
                isNewGame = false;
            }
            SendFemaleHome();
        }
        private void CanHeroLeadParty(Hero hero, ref bool result)
        {
            if (womenFolk.Contains(hero))
                result = false;
        }

        private void OnHeroCreated(Hero hero, bool bornNaturally)
        {
            if (hero.IsFemale && !AvoidQuestHeroChangesPatch.ToAvoid &&
                _onlyMaleOccupations?.Contains(hero.Occupation) == true)
            {
                UpdateSingleHero(hero, hero.Occupation);
            }
        }

        private void AfterSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            SendFemaleHome();

            if (!isNewGame)
                ShowInquiry();
        }

        private void ShowInquiry()
        {
            if (!UpdatedHeroGenders)
            {
                var inquiryElements = new List<InquiryElement>()
                {
                    new(Occupation.Wanderer, "Wanderers (Companions)", null, true, ""),
                    new(Occupation.GangLeader, "Gang Leader", null, true, ""),
                    new(Occupation.RuralNotable, "Rural Notable", null, true, ""),
                    new(Occupation.Artisan, "Artisan", null, true, ""),
                    new(Occupation.Headman, "Headman", null, true, ""),
                    new(Occupation.GoodsTrader, "Goods Trader", null, true, ""),
                };

                MBInformationManager.ShowMultiSelectionInquiry(new MultiSelectionInquiryData("Realistic Genders",
                    "Which occupations must have only male characters ? (Irreversible)", inquiryElements, true, 0,
                    inquiryElements.Count,
                    "Done", "Exit", UpdateGenders, elements => UpdatedHeroGenders = true));
            }
        }

        private void UpdateSingleHero(Hero hero, Occupation occupation)
        {
            List<Hero> maleTemplates = Hero.AllAliveHeroes.FindAll(x =>
                x.Occupation == occupation &&
                (x.Template?.IsFemale == false || !x.IsFemale));
            hero.UpdatePlayerGender(false);
            Hero template = hero;
            if (hero.Template.IsFemale)
            {
                if (maleTemplates?.IsEmpty() == true)
                {
                    InformationManager.DisplayMessage(new InformationMessage(
                        $"No male templates found at {Enum.GetName(typeof(Occupation), occupation)} occupation, using default hero names.",
                        Color.FromUint(11936259)));
                    template = Hero.AllAliveHeroes.GetRandomElementWithPredicate(x =>
                        !x.IsTemplate && !x.IsFemale);
                }
                else
                    template = maleTemplates.GetRandomElement();
            }

            NameGenerator.Current.GenerateHeroNameAndHeroFullName(template, out TextObject firstName,
                out TextObject fullName);
            hero.SetName(fullName, firstName);
        }

        private void UpdateGenders(List<InquiryElement> elements)
        {
            List<Occupation> chosenOccupations = new();
            foreach (var element in elements)
            {
                chosenOccupations.Add((Occupation)element.Identifier);

                List<Hero> heroesList = Hero.AllAliveHeroes.FindAll(x =>
                    x.Occupation == (Occupation)element.Identifier && x.IsFemale && x.Culture?.MaleNameList != null);

                heroesList.AddRange(Hero.DeadOrDisabledHeroes.FindAll(x =>
                    x.Occupation == (Occupation)element.Identifier && x.IsFemale && x.Culture?.MaleNameList != null));

                foreach (var hero in heroesList)
                {
                    UpdateSingleHero(hero, (Occupation)element.Identifier);
                }
            }

            _onlyMaleOccupations = chosenOccupations;
            UpdatedHeroGenders = true;
        }

        private void SendFemaleHome()
        {
            foreach (MobileParty mobileParty in MobileParty.All.Where(IsFemaleAndActiveParty).ToList())
            {
                Hero leader = mobileParty.LeaderHero;
                Settlement home = leader.HomeSettlement;
                if (home != null && leader != null)
                {
                    if (!CanFemaleLordLead(leader))
                    {
                        if (womenFolk.Contains(leader))
                        {
                            if (mobileParty.MapEvent == null)
                            {
                                Hero owner = mobileParty.LeaderHero ?? mobileParty.Owner;
                                Army? army = mobileParty.Army;
                                
                                if (army?.LeaderParty == mobileParty)
                                    DisbandArmyAction.ApplyByLeaderPartyRemoved(army);
                                
                                mobileParty.Army = null;

                                if (mobileParty.CurrentSettlement == home)
                                {
                                    List<TroopRosterElement> membersList = mobileParty.MemberRoster.GetTroopRoster();
                                    for (int k = 1; k < membersList.Count; k++)
                                    {
                                        TroopRosterElement t = membersList[k];
                                        home?.Town?.GarrisonParty?.MemberRoster?.AddToCounts(t.Character, t.Number,
                                            false, t.WoundedNumber, t.Xp, true, -1);
                                    }

                                    foreach (ItemRosterElement item in mobileParty.ItemRoster)
                                        SellItemsAction.Apply(mobileParty.Party, home.Party, item, item.Amount, home);
                                    
                                    GiveGoldAction.ApplyForPartyToSettlement(mobileParty.Party, home, (int)(owner?.Gold / 1.2));
                                    DestroyPartyAction.Apply(null, mobileParty);
                                }
                                else if (mobileParty.TargetSettlement != home || mobileParty.Ai.IsDisabled)
                                {
                                    mobileParty.Ai.EnableAi();
                                    mobileParty.Ai.SetMoveGoToSettlement(home);
                                }
                            }
                        }
                        else
                            womenFolk.Add(leader);
                    }
                    else
                    {
                        CheckIfNeedsToResetEquipment(leader);
                        if (womenFolk.Contains(leader))
                            womenFolk.Remove(leader);
                    }

                    
                }
            }
        }

        private void CheckIfNeedsToResetEquipment(Hero hero)
        {
            if (hero.BattleEquipment[EquipmentIndex.Weapon0].IsEmpty)
            {
                string[] equipmentIds = null;
                switch (hero.Culture.GetCultureCode())
                {
                    case CultureCode.Empire:
                        equipmentIds = new[]
                        {
                            "emp_bat_template_lady",
                            "emp_bat_template_medium"
                        };
                        break;
                    case CultureCode.Vlandia:
                        equipmentIds = new[]
                        {
                            "vla_bat_template_heavy",
                            "vla_bat_template_medium"
                        };
                        break;
                    case CultureCode.Sturgia:
                        equipmentIds = new[]
                        {
                            "stu_bat_template_heavy",
                            "stu_bat_template_lady",
                            "stu_bat_template_medium"
                        };
                        break;
                    case CultureCode.Aserai:
                        equipmentIds = new[]
                        {
                            "ase_bat_template_lady",
                            "ase_bat_template_medium",
                        };
                        break;
                    case CultureCode.Khuzait:
                        equipmentIds = new[]
                        {
                            "khu_bat_template_medium",
                            "khu_bat_template_lady",
                            "khu_bat_template_heavy",
                        };
                        break;
                    case CultureCode.Battania:
                        equipmentIds = new[]
                        {
                            "bat_bat_template_medium",
                            "bat_bat_template_heavy",
                            "bat_bat_template_lady",
                        };
                        break;
                }

                var equipment = MBObjectManager.Instance.GetObjectTypeList<MBEquipmentRoster>()
                    .Where(x => equipmentIds.Contains(x.StringId)).ToList().GetRandomElement();
                EquipmentHelper.AssignHeroEquipmentFromEquipment(hero, equipment.GetBattleEquipments().GetRandomElementInefficiently());
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData("UpdatedHeroGenders", ref UpdatedHeroGenders);
            dataStore.SyncData("_onlyMaleOccupations", ref _onlyMaleOccupations);
            
            if(onlyMaleOccupations?.Length > 0)
                dataStore.SyncData("onlyMaleOccupations", ref onlyMaleOccupations);
        }
    }
}