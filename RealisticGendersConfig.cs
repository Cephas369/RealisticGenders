using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using TaleWorlds.ModuleManager;

namespace RealisticGenders
{
    internal class RealisticGendersConfig : AttributeGlobalSettings<RealisticGendersConfig>
    {
        public override string Id => "realistic_genders";
        public override string DisplayName => $"Realistic Genders {Regex.Replace(ModuleHelper.GetModuleInfo(Assembly.GetExecutingAssembly().GetName().Name).Version.ToString(), @"v|\.0$", string.Empty)}";
        public override string FolderName => "RealisticGenders";
        public override string FormatType => "json";

        [SettingPropertyGroup("General")]

        [SettingPropertyBool("Spawn peasant women at sieges", Order = 1, RequireRestart = false, HintText = "In medieval times sieges were mainly carried out by surprise, with children and women inside and this forced women to protect themselves with their husbands instead of hoping for the best.")]
        public bool SpawnWomenAtSieges { get; set; } = true;

        [SettingPropertyGroup("General")]

        [SettingPropertyBool("Disable condition for female lords to lead parties", Order = 2, RequireRestart = false, HintText = "Enabling this option will make all female lords including the ones with no females unable to lead parties.")]
        public bool DisableFemaleCondition { get; set; } = false;

        [SettingPropertyGroup("General")]

        [SettingPropertyBool("Allow female rulers to lead parties.", Order = 3, RequireRestart = false, HintText = "")]
        public bool AllowRulersToLead { get; set; } = true;

    }
}