using Comfort.Common;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using Systems.Effects;
using AssetBundleLoader;

namespace ultravisceral
{
    public class PatchDamage : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BodyPartCollider), nameof(BodyPartCollider.ApplyHit));
        }

        [PatchPostfix]
        private static void PatchPostfix(ref BodyPartCollider __instance, DamageInfoStruct damageInfo, ShotIdStruct shotID)
        {
            if (__instance.Player != null && __instance.Player.IsYourPlayer)
                return;

            Play(damageInfo, shotID);
        }

        public static void Play(DamageInfoStruct damageInfo, ShotIdStruct shotID)
        {
            ParticleEffectManager.Instance.PlayBloodEffect(damageInfo.HitPoint, damageInfo.HitNormal, Mathf.Max(damageInfo.Damage, 100));

            BFXManager.Instance.Play(damageInfo.HitPoint, damageInfo.Direction);

            AudioClip[] hitClips = BundleLoader.LoadAssetBundle("ultrablood").LoadAllAssets<AudioClip>();
            Singleton<BetterAudio>.Instance.PlayAtPoint(damageInfo.HitPoint, hitClips[Random.Range(0, hitClips.Length)], BetterAudio.AudioSourceGroupType.Collisions, 100);

            Singleton<Effects>.Instance.EmitBloodOnEnvironment(damageInfo.HitPoint, damageInfo.HitNormal);
        }
    }
}
