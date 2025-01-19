using AssetBundleLoader;
using System.Collections.Generic;
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
            trailMaterial = new Material(Shader.Find("Sprites/Default"));
            //new Material(BundleLoader.LoadAssetBundle("ultrablood").LoadAsset<Shader>("Additive"));
            mainMaterial = new Material(Shader.Find("Sprites/Default"));
            mainMaterial.mainTexture = BundleLoader.LoadAssetBundle("ultrablood").LoadAsset<Texture>("circle16");

            for (int i = 0; i < PoolSize; i++)
            {
                particlePool.Enqueue(CreateParticleSystem());
            }
        }

        public void PlayBloodEffect(Vector3 position, Vector3 normal, float strength)
        {
            ParticleSystem particleSystem = GetParticleSystem();
            SetEffectBlood(particleSystem, 2, true);
            Transform psTransform = particleSystem.transform;
            psTransform.position = position;
            psTransform.rotation = Quaternion.LookRotation(-normal);
            particleSystem.gameObject.SetActive(true);
            particleSystem.Play();
            StartCoroutine(RecycleAfterFinish(particleSystem));

            particleSystem = GetParticleSystem();
            SetEffectBlood(particleSystem, 10, true, (int)(strength / 5f));
            psTransform = particleSystem.transform;
            psTransform.position = position;
            psTransform.rotation = Quaternion.LookRotation(normal);
            particleSystem.gameObject.SetActive(true);
            particleSystem.Play();
            StartCoroutine(RecycleAfterFinish(particleSystem));

            particleSystem = GetParticleSystem();
            SetEffectBlood(particleSystem, 3, false);
            psTransform = particleSystem.transform;
            psTransform.position = position;
            psTransform.rotation = Quaternion.LookRotation(normal);
            particleSystem.gameObject.SetActive(true);
            particleSystem.Play();
            StartCoroutine(RecycleAfterFinish(particleSystem));
        }

        private void SetEffectBlood(ParticleSystem ps, float startSpeed, bool trailsEnabled, int amount = 40)
        {
            var main = ps.main;
            main.loop = false;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.3f, 0, 0, 1), new Color(0.05f, 0, 0, 1));
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
            shape.angle = 90f;
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
        }

        ParticleSystem GetParticleSystem()
        {
            if (particlePool.Count > 0)
            {
                return particlePool.Dequeue();
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
            particleRenderer.trailMaterial.color = new Color(1,1,1,0.8f);

            var collision = ps.collision;
            collision.enabled = true;
            collision.type = ParticleSystemCollisionType.World;
            collision.collidesWith = 1 << 18;
            collision.bounce = 0.2f;
            collision.dampen = 0.9f;

            ps.Stop();

            return ps;
        }

        private System.Collections.IEnumerator RecycleAfterFinish(ParticleSystem ps)
        {
            yield return new WaitForSeconds(ps.main.startLifetime.constantMax);

            ps.transform.SetParent(null);
            ps.Stop();

            particlePool.Enqueue(ps);
        }
    }
}
