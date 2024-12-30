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

        private void Init()
        {
            for (int i = 0; i < PoolSize; i++)
            {
                particlePool.Enqueue(CreateParticleSystem());
            }
        }

        public void PlayBloodEffect(Vector3 position, Vector3 normal)
        {
            ParticleSystem particleSystem = GetParticleSystem();
            SetEffectBlood(particleSystem, 2, true);
            Transform psTransform = particleSystem.transform;
            psTransform.position = position;
            psTransform.rotation = Quaternion.LookRotation(normal);
            particleSystem.gameObject.SetActive(true);
            particleSystem.Play();
            StartCoroutine(RecycleAfterFinish(particleSystem));

            particleSystem = GetParticleSystem();
            SetEffectBlood(particleSystem, 10, true);
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

        private void SetEffectBlood(ParticleSystem ps, float startSpeed, bool trailsEnabled)
        {
            var main = ps.main;
            main.loop = false;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0, 0, 1), new Color(0.5f, 0, 0, 1));
            main.startSize = 0.1f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(startSpeed, startSpeed * 3f);
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.1f, 2f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.5f;

            var emission = ps.emission;
            emission.rateOverTime = 0f; // No continuous emission
            var burst = new ParticleSystem.Burst(0f, 30);
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
            trails.ratio = 1.0f;
            trails.mode = ParticleSystemTrailMode.PerParticle;
            trails.lifetime = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            trails.dieWithParticles = false;

            trails.textureMode = ParticleSystemTrailTextureMode.Stretch;
            trails.widthOverTrail = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0, 1f), new Keyframe(1, 0)));
            trails.colorOverTrail = new ParticleSystem.MinMaxGradient(Color.red, new Color(0.5f, 0f, 0f, 0f));
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

            var particleRenderer = bloodEffect.GetComponent<ParticleSystemRenderer>();
            Material trailMaterial = new Material(Shader.Find("Sprites/Default"));
            particleRenderer.material = trailMaterial;
            particleRenderer.trailMaterial = trailMaterial;
            particleRenderer.trailMaterial.color = Color.white;

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
