using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;
using Systems.Effects;
using DeferredDecals;
using EFT.Ballistics;
using static DeferredDecals.DeferredDecalRenderer;
using AssetBundleLoader;

namespace ultravisceral
{
    public class PatchDamage : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BodyPartCollider), nameof(BodyPartCollider.ApplyHit));
        }

        static SingleDecal[] decals;

        static SingleDecal GetRandomDecal()
        {
            return decals[Random.Range(0, decals.Length)];
        }

        static void Init()
        {
            AssetBundle bundle = BundleLoader.LoadAssetBundle("ultrablood");
            Texture[] textures = bundle.LoadAllAssets<Texture>();
            decals = new SingleDecal[textures.Length];

            for (int i = 0; i < textures.Length; i++)
            {
                SingleDecal decal = new SingleDecal();

                Material mat = new Material(Shader.Find("Sprites/Default"));
                mat.mainTexture = textures[i];

                decal.DecalSize = new Vector2(1f, 1f) * Random.Range(0.4f, 0.9f);
                decal.DecalMaterial = mat;
                decal.DynamicDecalMaterial = decal.DecalMaterial;
                decal.Init();

                decals[i] = decal;
            }
        }

        [PatchPostfix]
        private static void PatchPostfix(ref BodyPartCollider __instance, DamageInfoStruct damageInfo, ShotIdStruct shotID)
        {
            if (__instance.Player.IsYourPlayer)
                return;

            if (decals == null)
                Init();

            ParticleEffectManager.Instance.PlayBloodEffect(damageInfo.HitPoint, damageInfo.HitNormal);

            if (Physics.Raycast(damageInfo.HitPoint, Vector3.down, out RaycastHit hit, 4f, 1 << 12))
            {
                if (hit.transform.TryGetComponent<BallisticCollider>(out BallisticCollider col))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        SingleDecal decal = GetRandomDecal();
                        Singleton<Effects>.Instance.DeferredDecals.method_5(hit.point + new Vector3(Random.value - 0.5f, Random.value - 0.5f), Vector3.up, col, decal, decal.DynamicDecalMaterial, 0);

                        Singleton<Effects>.Instance.EmitBleeding(hit.point, hit.normal);
                    }
                }
            }

            if (Physics.Raycast(damageInfo.HitPoint, -damageInfo.HitNormal, out RaycastHit hit2, 4f, 1 << 12))
            {
                if (hit2.transform.TryGetComponent<BallisticCollider>(out BallisticCollider col))
                {
                    SingleDecal decal = GetRandomDecal();
                    Singleton<Effects>.Instance.DeferredDecals.method_5(hit2.point, hit2.normal, col, decal, decal.DynamicDecalMaterial, 0.05f);
                }
            }
        }
    }
}
