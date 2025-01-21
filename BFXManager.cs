using AssetBundleLoader;
using Comfort.Common;
using System.Collections.Generic;
using System.Reflection;
using Systems.Effects;
using UnityEngine;

namespace ultravisceral
{
    internal class BFXManager : MonoBehaviour
    {
        public static BFXManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("BFXManager").AddComponent<BFXManager>();
                    _instance.Init();
                }
                return _instance;
            }
        }
        private static BFXManager _instance;
        private List<GameObject> spawnedObjects = new List<GameObject>();

        public void Init()
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

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                Clear();
            }
        }

        public void Play(Vector3 point, Vector3 direction)
        {
            GameObject gameObject6 = Object.Instantiate<GameObject>(BundleLoader.LoadAssetBundle("bloodfx.bundle").LoadAllAssets<GameObject>()[Random.Range(1, 17)]);
            GameObject gameObject3 = Object.Instantiate<GameObject>(BundleLoader.LoadAssetBundle("bloodfx.bundle").LoadAllAssets<GameObject>()[18]);
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

            spawnedObjects.Add(gameObject6);
            spawnedObjects.Add(gameObject4);
            spawnedObjects.Add(gameObject3);
        }

        public void Clear()
        {
            for (int i = 0; i < spawnedObjects.Count; i++)
            {
                GameObject.Destroy(spawnedObjects[i]);
            }

            spawnedObjects.Clear();
        }
    }
}
