using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Samples
{
    public class Target : MonoBehaviour
    {
        [Range(0, 4)]
        public int forSide;
        [Range(-3, 3)]
        public float rotateSpeed;
        [Range(0.1f, 3f)]
        public float moveInterval;
        [Range(1f, 10f)]
        public float waitInterval;

        private void Awake()
        {
            Material mat;
            if (GameManager.Instance.matsBySide.TryGetValue(this.forSide, out mat))
            {
                GetComponent<MeshRenderer>().material = mat;
            }
        }

        private void OnEnable()
        {
            StartCoroutine(DoMove());
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private IEnumerator DoMove()
        {
            while(true)
            {
                float t = 0;
                while(t < moveInterval)
                {
                    float delta = Time.deltaTime;
                    t += delta;

                    Vector3 pos = transform.localPosition;
                    pos.y = 0;
                    float r = pos.magnitude;
                    float angle = Mathf.Atan2(pos.z, pos.x);
                    angle += rotateSpeed * delta;
                    pos.x = r * Mathf.Cos(angle);
                    pos.z = r * Mathf.Sin(angle);
                    transform.localPosition = pos;

                    yield return null;
                }
                yield return new WaitForSeconds(waitInterval);
            }
        }
    }
}
