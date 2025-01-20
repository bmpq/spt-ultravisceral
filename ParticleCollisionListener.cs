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

        private float minDistanceBetweenDecals = 1f;
        List<Vector3> lastCollisionPoints = new List<Vector3>();

        void Start()
        {
            ps = GetComponent<ParticleSystem>();
            collisionEvents = new List<ParticleCollisionEvent>();
        }

        void OnEnable()
        {
            lastCollisionPoints.Clear();
        }

        void OnParticleCollision(GameObject other)
        {
            int numCollisionEvents = ps.GetCollisionEvents(other, collisionEvents);

            for (int i = 0; i < Mathf.Min(numCollisionEvents, collisionEvents.Count); i++)
            {
                Vector3 collisionPoint = collisionEvents[i].intersection;
                bool canDrawDecal = true;

                foreach (Vector3 lastPoint in lastCollisionPoints)
                {
                    if (Vector3.Distance(collisionPoint, lastPoint) < Plugin.MinDistanceDecals.Value)
                    {
                        canDrawDecal = false;
                        break;
                    }
                }

                if (canDrawDecal)
                {
                    Singleton<Effects>.Instance.DeferredDecals.DrawDecal(collisionPoint, collisionEvents[i].normal, null, false);
                    lastCollisionPoints.Add(collisionPoint);
                }
            }
        }
    }
}
