using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ultravisceral;

namespace ultravisceral
{
    public class PatchDamage : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.ApplyDamageInfo));
        }

        [PatchPostfix]
        private static void PatchPostfix(ref Player __instance, DamageInfoStruct damageInfo, EBodyPart bodyPartType)
        {
            if (__instance.IsYourPlayer)
                return;

            ParticleEffectManager.Instance.PlayBloodEffect(damageInfo.HitPoint, damageInfo.HitNormal);
        }
    }
}
