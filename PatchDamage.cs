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

        static AudioClip[] hitClips;

        public static void Init()
        {
            AssetBundle bundle = BundleLoader.LoadAssetBundle("ultrablood");
            Texture texSheet = bundle.LoadAsset<Texture>("BloodSheet");

            FieldInfo field4 = typeof(DeferredDecalRenderer).GetField("_genericDecal", BindingFlags.Instance | BindingFlags.NonPublic);
            SingleDecal genericDecal = (SingleDecal)field4.GetValue(Singleton<Effects>.Instance.DeferredDecals);

            SingleDecal decal = new SingleDecal();

            Material mat = genericDecal.DecalMaterial;
            mat.mainTexture = texSheet;
            mat.color = new Color(0.5f, 0.5f, 0.5f, 1f);

            decal.TileSheetRows = 1;
            decal.TileSheetColumns = 10;

            decal.DecalSize = new Vector2(1f, 1f);
            decal.DecalMaterial = mat;
            decal.DynamicDecalMaterial = decal.DecalMaterial;
            decal.DecalMaterialType = new MaterialType[] { MaterialType.None };
            decal.Init();

            field4.SetValue(Singleton<Effects>.Instance.DeferredDecals, decal);

            hitClips = bundle.LoadAllAssets<AudioClip>();

            InitBloodFX();
        }

        [PatchPostfix]
        private static void PatchPostfix(ref BodyPartCollider __instance, DamageInfoStruct damageInfo, ShotIdStruct shotID)
        {
            if (__instance.Player != null && __instance.Player.IsYourPlayer)
                return;

            ParticleEffectManager.Instance.PlayBloodEffect(damageInfo.HitPoint, damageInfo.HitNormal);

            SpawnBlood(damageInfo.HitPoint, damageInfo.Direction);

            Singleton<BetterAudio>.Instance.PlayAtPoint(damageInfo.HitPoint, hitClips[Random.Range(0, hitClips.Length)], BetterAudio.AudioSourceGroupType.Collisions, 100);

            Singleton<Effects>.Instance.EmitBloodOnEnvironment(damageInfo.HitPoint, damageInfo.HitNormal);

            for (int i = 0; i < 12; i++)
            {
                if (Physics.Raycast(damageInfo.HitPoint, Random.onUnitSphere, out RaycastHit hit, 3f, 1 << 12))
                {
                    Singleton<Effects>.Instance.DeferredDecals.DrawDecal(hit, null);
                }
            }
        }

        static void InitBloodFX()
        {
            TextureDecalsPainter texDecals = Singleton<Effects>.Instance.TexDecals;
            FieldInfo field = typeof(TextureDecalsPainter).GetField("_decalSize", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo field2 = typeof(TextureDecalsPainter).GetField("_bloodDecalTexture", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo field3 = typeof(TextureDecalsPainter).GetField("_vestDecalTexture", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo field4 = typeof(TextureDecalsPainter).GetField("_backDecalTexture", BindingFlags.Instance | BindingFlags.NonPublic);
            Vector2 vector = new Vector2(0.25f, 0.35f);
            field.SetValue(texDecals, vector);
            object value = field2.GetValue(texDecals);
            field3.SetValue(texDecals, value);
            field4.SetValue(texDecals, value);
            GameObject gameObject = BundleLoader.LoadAssetBundle("bloodsfx.bundle").LoadAllAssets<GameObject>()[1];
            GameObject gameObject2 = BundleLoader.LoadAssetBundle("bloodfx.bundle").LoadAllAssets<GameObject>()[1];
            gameObject2.transform.position = new Vector3(0f, -9999999f, 0f);
            gameObject.transform.position = new Vector3(0f, -9999999f, 0f);
            Object.Instantiate<GameObject>(gameObject);
            Object.Instantiate<GameObject>(gameObject2);
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
            var main = gameObject4.GetComponent<ParticleSystem>().main;
            main.scalingMode = ParticleSystemScalingMode.Local;
            ParticleSystem[] componentsInChildren = gameObject4.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                var m = componentsInChildren[i].main;
                m.scalingMode = ParticleSystemScalingMode.Hierarchy;
            }
            main.loop = false;
            main.gravityModifier = 12f;
            main.startColor = new Color(0.1509f, 0f, 0f, 1f);
            gameObject4.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        }
    }
}
