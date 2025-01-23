using AssetBundleLoader;
using Comfort.Common;
using System;
using System.Collections.Generic;
using Systems.Effects;
using UnityEngine;

namespace ultravisceral
{
    public class ParticleEffectManager : MonoBehaviour
    {
        public static ParticleEffectManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("ParticleEffectManager").AddComponent<ParticleEffectManager>();
                    _instance.Init();
                }
                return _instance;
            }
        }
        private static ParticleEffectManager _instance;

        private Queue<ParticleSystem> particlePool = new Queue<ParticleSystem>();
        private const int PoolSize = 10;

        Material mainMaterial;
        Material trailMaterial;

        private void Init()
        {
            Shader particleShader = BundleLoader.LoadAssetBundle("ultrablood").LoadAsset<Shader>("ParticleDiffuse");

            mainMaterial = new Material(particleShader);
            mainMaterial.mainTexture = BundleLoader.LoadAssetBundle("ultrablood").LoadAsset<Texture>("circle16");
            mainMaterial.SetFloat("_Metallic", 0.6f);
            mainMaterial.SetFloat("_Smoothness", 0.4f);

            trailMaterial = new Material(particleShader);
            trailMaterial.SetFloat("_Metallic", 0.6f);
            trailMaterial.SetFloat("_Smoothness", 0.4f);
            trailMaterial.color = new Color(1, 1, 1, 0.95f);

            for (int i = 0; i < PoolSize; i++)
            {
                particlePool.Enqueue(CreateParticleSystem());
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                Singleton<Effects>.Instance.DeferredDecals.Clear();
            }
        }

        public void PlayBloodEffect(Vector3 position, Vector3 normal, float strength)
        {
            ParticleSystem particleSystem = GetParticleSystem();
            SetEffectBlood(particleSystem, 2, true);
            Transform psTransform = particleSystem.transform;
            psTransform.position = position;
            psTransform.rotation = Quaternion.LookRotation(-normal);
            particleSystem.Play();
            StartCoroutine(RecycleAfterFinish(particleSystem));

            particleSystem = GetParticleSystem();
            SetEffectBlood(particleSystem, 10, true, (int)(strength / 5f));
            psTransform = particleSystem.transform;
            psTransform.position = position;
            psTransform.rotation = Quaternion.LookRotation(normal);
            particleSystem.Play();
            StartCoroutine(RecycleAfterFinish(particleSystem));

            particleSystem = GetParticleSystem();
            SetEffectBlood(particleSystem, 3, false);
            psTransform = particleSystem.transform;
            psTransform.position = position;
            psTransform.rotation = Quaternion.LookRotation(normal);
            particleSystem.Play();
            StartCoroutine(RecycleAfterFinish(particleSystem));
        }

        private void SetEffectBlood(ParticleSystem ps, float startSpeed, bool trailsEnabled, int amount = 40)
        {
            var main = ps.main;
            main.loop = false;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0, 0, 1), new Color(0.5f, 0, 0, 0.9f));
            main.startSize = 0.07f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(startSpeed, startSpeed * 3f);
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.1f, 2f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = ps.emission;
            emission.rateOverTime = 0f; // No continuous emission
            var burst = new ParticleSystem.Burst(0f, amount);
            emission.SetBursts(new ParticleSystem.Burst[] { burst });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 140f;
            shape.radius = 0.05f;
            shape.scale = Vector3.one;

            var limit = ps.limitVelocityOverLifetime;
            limit.enabled = false;

            var trails = ps.trails;
            trails.enabled = trailsEnabled;
            trails.ratio = 0.9f;
            trails.mode = ParticleSystemTrailMode.PerParticle;
            trails.lifetime = new ParticleSystem.MinMaxCurve(0f, 0.1f);
            trails.dieWithParticles = true;

            trails.textureMode = ParticleSystemTrailTextureMode.Stretch;
            trails.inheritParticleColor = true;
            trails.generateLightingData = true;
        }

        ParticleSystem GetParticleSystem()
        {
            if (particlePool.Count > 0)
            {
                var ps = particlePool.Dequeue();
                ps.gameObject.SetActive(true);
                return ps;
            }
            else
            {
                return CreateParticleSystem();
            }
        }

        private ParticleSystem CreateParticleSystem()
        {
            GameObject bloodEffect = new GameObject("ParticleEffect");
            ParticleSystem ps = bloodEffect.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.gravityModifier = 3f;

            var particleRenderer = bloodEffect.GetComponent<ParticleSystemRenderer>();

            particleRenderer.material = mainMaterial;
            particleRenderer.trailMaterial = trailMaterial;

            var collision = ps.collision;
            collision.enabled = true;
            collision.type = ParticleSystemCollisionType.World;
            collision.collidesWith = 1 << 11 | 1 << 18;
            collision.dampen = 1f;
            collision.lifetimeLoss = 1f;
            collision.sendCollisionMessages = true;
            collision.enableDynamicColliders = false;

            ps.Stop();

            bloodEffect.AddComponent<ParticleCollisionListener>();

            return ps;
        }

        private System.Collections.IEnumerator RecycleAfterFinish(ParticleSystem ps)
        {
            yield return new WaitForSeconds(ps.main.startLifetime.constantMax);

            ps.Stop();
            ps.gameObject.SetActive(false);
            ps.transform.SetParent(null);

            particlePool.Enqueue(ps);
        }
    }
}
