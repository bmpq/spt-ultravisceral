using UnityEngine;

namespace ultravisceral
{
    public class randomRotate : MonoBehaviour
    {
        public float rotateEverySecond = 1f;

        void Start()
        {
            transform.rotation = Random.rotation;
        }

        void Update()
        {

        }
    }
}