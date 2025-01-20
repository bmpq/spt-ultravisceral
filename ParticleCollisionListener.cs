using Comfort.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Systems.Effects;
using UnityEngine;

namespace ultravisceral
{
    internal class ParticleCollisionListener : MonoBehaviour
    {
        ParticleSystem ps;
        List<ParticleCollisionEvent> collisionEvents;

        void Start()
        {
            ps = GetComponent<ParticleSystem>();
            collisionEvents = new List<ParticleCollisionEvent>();
        }

        void OnParticleCollision(GameObject other)
        {
            int numCollisionEvents = ps.GetCollisionEvents(other, collisionEvents);

            int i = 0;

            while (i < numCollisionEvents)
            {
                if (collisionEvents[i].velocity.magnitude > 0.1f)
                {
                    Singleton<Effects>.Instance.DeferredDecals.DrawDecal(collisionEvents[i].intersection, collisionEvents[i].normal, null, false);
                }
                i++;
            }
        }
    }
}
