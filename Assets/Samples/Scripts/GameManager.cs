using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using AillieoUtils;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Samples
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;

        public Material[] mats;

        public Simulator simulator { get; private set; }
        public readonly Dictionary<int, Target> targetsBySide = new Dictionary<int, Target>();
        public readonly Dictionary<int, Material> matsBySide = new Dictionary<int, Material>();
        private readonly TimeCostRecorder recorder = new TimeCostRecorder();

        [SerializeField]
        private Config OACconfig;

        void Awake()
        {
            if(Instance != null)
            {
                return;
            }

            Instance = this;

            simulator = new Simulator(OACconfig);

            for(int i = 0, len= mats.Length; i < len; ++i)
            {
                matsBySide[i] = mats[i];
            }

            Target[] targets = FindObjectsOfType<Target>();

            foreach (var t in targets)
            {
                targetsBySide[t.forSide] = t;
            }
        }

        void Update()
        {
            recorder.Start();
            simulator.Step(Time.deltaTime);
            recorder.Stop();
        }

        void CreateNew()
        {
            Debug.Log("not implement");
        }

        void RemoveRandom()
        {
            Debug.Log("not implement");
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(GameManager))]
        public class GameManagerEditor : Editor
        {

            public override bool RequiresConstantRepaint()
            {
                return Application.isPlaying;
            }

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                EditorGUILayout.Space();
                GUILayout.BeginVertical("box");
                GameManager gameManager = target as GameManager;
                GUILayout.Label($"SimuAvgCost(ms) = {gameManager.recorder.GetTimeCostAvgMS()}");
                GUILayout.Label($"SimuMaxCost(ms) = {gameManager.recorder.GetTimeCostMaxMS()}");
                GUILayout.Label($"SimuMinCost(ms) = {gameManager.recorder.GetTimeCostMinMS()}");
                float fail = gameManager.simulator != null ? 100 * gameManager.simulator.GetFailureRate() : 0;
                GUILayout.Label($"SimuFailureRate(%) = {fail:f4}");
                GUILayout.EndVertical();

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Create New"))
                {
                    gameManager.CreateNew();
                }
                if (GUILayout.Button("Remove Random"))
                {
                    gameManager.RemoveRandom();
                }
                GUILayout.EndHorizontal();
            }
        }

#endif

    }
}
