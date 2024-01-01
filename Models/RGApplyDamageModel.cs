using System;
using System.Collections.Generic;
using System.Text;
using SandBox.GameComponents;
using TaleWorlds.MountAndBlade;

namespace RealisticGenders.Models
{
    internal class RGApplyDamageModel : SandboxAgentApplyDamageModel
    {
        public override float CalculateDamage(in AttackInformation attackInformation, in AttackCollisionData collisionData,
            in MissionWeapon weapon, float baseDamage)
        {
            float baseValue = base.CalculateDamage(in attackInformation, in collisionData, in weapon, baseDamage);
            MissionWeapon attackWeapon = weapon;
            if (attackInformation.AttackerAgent?.IsFemale == true && !attackWeapon.IsAnyAmmo() && !attackWeapon.IsAnyConsumable())
                return baseValue * 0.6f;
            return baseValue;
        }
    }
}
