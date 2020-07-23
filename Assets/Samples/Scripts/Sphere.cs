using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AillieoUtils;

namespace Samples
{
    public class Sphere : MonoBehaviour
    {
        [Range(0, 4)]
        public int side = 0;
        [Range(1, 2)]
        public float scale = 1f;
        [Range(1f, 4f)]
        public float speed = 1f;

        private Target target;
        private Agent agent;


        private void Awake()
        {
            if(!GameManager.Instance.targetsBySide.TryGetValue(side, out this.target))
            {
                Debug.LogError($"没有匹配的target side={side}");
                return;
            }

            transform.localScale = Vector3.one * scale;
            Vector3 position = transform.localPosition;
            position.y = scale / 2;
            transform.localPosition = position;
            Material mat;
            if(GameManager.Instance.matsBySide.TryGetValue(this.side, out mat))
            {
                GetComponent<MeshRenderer>().material = mat;
            }
        }


        private void OnEnable()
        {
            agent = GameManager.Instance.simulator.CreateAgent();
            agent.position = transform.localPosition.ToAVec();
            agent.speed = speed;
            agent.radius = scale / 2f;
            agent.goal = target.transform.localPosition.ToAVec();
        }

        private void OnDisable()
        {
            GameManager.Instance.simulator.RemoveAgent(agent.id);
            this.agent = null;
        }

        private void Update()
        {
            Vector3 position = transform.localPosition;
            position.x = agent.position.x;
            position.z = agent.position.y;
            transform.localPosition = position;
            agent.goal = target.transform.localPosition.ToAVec();
        }
    }

}