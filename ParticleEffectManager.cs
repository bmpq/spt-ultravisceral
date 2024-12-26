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

            SetEffectBlood(particleSystem, 2);

            Transform psTransform = particleSystem.transform;
            psTransform.position = position;
            psTransform.rotation = Quaternion.LookRotation(normal);

            particleSystem.gameObject.SetActive(true);
            particleSystem.Play();

            StartCoroutine(RecycleAfterFinish(particleSystem));

            particleSystem = GetParticleSystem();

            SetEffectBlood(particleSystem, 10);

            psTransform = particleSystem.transform;
            psTransform.position = position;
            psTransform.rotation = Quaternion.LookRotation(normal);

            particleSystem.gameObject.SetActive(true);
            particleSystem.Play();

            StartCoroutine(RecycleAfterFinish(particleSystem));
        }

        private void SetEffectBlood(ParticleSystem ps, float startSpeed)
        {
            var main = ps.main;
            main.loop = false;
            main.startColor = Color.blue;
            main.startSize = 0.1f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(startSpeed, startSpeed * 2f);
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.1f, 2f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.5f;

            var emission = ps.emission;
            emission.rateOverTime = 0f; // No continuous emission
            var burst = new ParticleSystem.Burst(0f, 30);
            emission.SetBursts(new ParticleSystem.Burst[] { burst });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 40f;
            shape.radius = 0.3f;
            shape.scale = Vector3.one;

            var limit = ps.limitVelocityOverLifetime;
            limit.enabled = false;

            var trails = ps.trails;
            trails.enabled = true;
            trails.ratio = 1.0f;
            trails.mode = ParticleSystemTrailMode.PerParticle;
            trails.lifetime = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
            trails.dieWithParticles = false;

            trails.textureMode = ParticleSystemTrailTextureMode.Stretch;
            trails.widthOverTrail = new ParticleSystem.MinMaxCurve(2f, new AnimationCurve(new Keyframe(0, 1f), new Keyframe(1, 0)));
            trails.colorOverTrail = new ParticleSystem.MinMaxGradient(Color.blue, new Color(0.5f, 0f, 0f, 0f));
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
