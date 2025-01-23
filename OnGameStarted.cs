using EFT;
using EFT.CameraControl;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ultravisceral
{
    internal class OnGameStarted : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        public static void PatchPostfix(GameWorld __instance)
        {
            float dmg = 300f;

            DamageInfoStruct prewarmDamage = new DamageInfoStruct
            {
                DamageType = dmg > 200f ? EDamageType.Explosion : EDamageType.Bullet,
                Damage = dmg,
                ArmorDamage = dmg,
                StaminaBurnRate = dmg,
                PenetrationPower = dmg,
                Direction = UnityEngine.Random.onUnitSphere,
                HitNormal = UnityEngine.Random.onUnitSphere,
                HitPoint = new Vector3(0, 200, 0f),
                MasterOrigin = Vector3.zero,
                Player = __instance.GetAlivePlayerBridgeByProfileID(__instance.MainPlayer.ProfileId),
                IsForwardHit = true,

                BlockedBy = null,
                DeflectedBy = null
            };
            PatchDamage.Play(prewarmDamage, ShotIdStruct.EMPTY_SHOT_ID);
        }
    }
}
