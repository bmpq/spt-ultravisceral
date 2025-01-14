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

        static AudioClip[] hitClips;

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
                mat.color = new Color(0.5f, 0.5f, 0.5f, 1f);

                decal.DecalSize = new Vector2(1f, 1f) * Random.Range(0.2f, 0.9f);
                decal.DecalMaterial = mat;
                decal.DynamicDecalMaterial = decal.DecalMaterial;
                decal.Init();

                decals[i] = decal;
            }

            hitClips = bundle.LoadAllAssets<AudioClip>();
        }

        [PatchPostfix]
        private static void PatchPostfix(ref BodyPartCollider __instance, DamageInfoStruct damageInfo, ShotIdStruct shotID)
        {
            if (__instance.Player != null && __instance.Player.IsYourPlayer)
                return;

            if (decals == null)
                Init();

            ParticleEffectManager.Instance.PlayBloodEffect(damageInfo.HitPoint, damageInfo.HitNormal);

            SpawnBlood(damageInfo.HitPoint, damageInfo.Direction);

            Singleton<BetterAudio>.Instance.PlayAtPoint(damageInfo.HitPoint, hitClips[Random.Range(0, hitClips.Length)], BetterAudio.AudioSourceGroupType.Collisions, 100);

            if (Physics.Raycast(damageInfo.HitPoint, Vector3.down, out RaycastHit hit, 4f, 1 << 12))
            {
                if (hit.transform.TryGetComponent<BallisticCollider>(out BallisticCollider col))
                {
                    for (int i = 0; i < 5; i++)
                    {
                        SingleDecal decal = GetRandomDecal();
                        Singleton<Effects>.Instance.DeferredDecals.method_5(hit.point + new Vector3(Random.value - 0.5f * 2f, Random.value - 0.5f) * 2f, Vector3.up, col, decal, decal.DynamicDecalMaterial, 0);

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

        private static void SpawnBlood(Vector3 point, Vector3 direction)
        {
            int num = UnityEngine.Random.Range(1, 17);
            GameObject gameObject5 = BundleLoader.LoadAssetBundle("bloodfx.bundle").LoadAllAssets<GameObject>()[num];
            GameObject gameObject2 = BundleLoader.LoadAssetBundle("bloodfx.bundle").LoadAllAssets<GameObject>()[18];
            GameObject gameObject6 = Object.Instantiate<GameObject>(gameObject5);
            GameObject gameObject3 = Object.Instantiate<GameObject>(gameObject2);
            BFX_BloodSettings component = gameObject6.GetComponent<BFX_BloodSettings>();
            component.LightIntensityMultiplier = 3f;
            gameObject6.transform.position = point;
            gameObject6.transform.localScale = new Vector3(Plugin.BloodSplatterSize.Value, Plugin.BloodSplatterSize.Value, Plugin.BloodSplatterSize.Value);
            direction.y = 0f;
            Quaternion quaternion = Quaternion.LookRotation(direction);
            quaternion.y -= 180f;
            gameObject6.transform.rotation = quaternion;
            component.GroundHeight = point.y - 1.9f;
            gameObject3.transform.position = point;
            GameObject gameObject4 = Object.Instantiate<GameObject>(BundleLoader.LoadAssetBundle("blood_particles.bundle").LoadAllAssets<GameObject>()[7]);
            gameObject4.transform.position = point;
            gameObject4.transform.localPosition = new Vector3(0f, 0f, 0f);
            gameObject4.transform.localRotation = new Quaternion(-0.1163f, 0.8234f, -0.1825f, -0.5246f);
            gameObject4.GetComponent<ParticleSystem>().scalingMode = ParticleSystemScalingMode.Local;
            ParticleSystem[] componentsInChildren = gameObject4.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                componentsInChildren[i].scalingMode = ParticleSystemScalingMode.Hierarchy;
            }
            gameObject4.GetComponent<ParticleSystem>().loop = false;
            gameObject4.GetComponent<ParticleSystem>().gravityModifier = 12f;
            gameObject4.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            gameObject4.GetComponent<ParticleSystem>().startColor = new Color(0.1509f, 0f, 0f, 1f);
        }
    }
}
