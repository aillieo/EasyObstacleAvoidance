using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AillieoUtils;
using UnityEngine;
using UVector2 = UnityEngine.Vector2;
using AVector2 = AillieoUtils.Vector2;
using System.Diagnostics;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Samples
{
    public class TestKDTree : MonoBehaviour
    {
        public class PointWithState: IPositionProvider
        {
            public AVector2 pos;
            public Color color;
            public int lastUpdateFrame = -1;

            public AVector2 position => pos;

            public void SetSelected(bool selected)
            {
                this.color = selected ? Color.blue : Color.white;
            }
        }

        [Header("Target spheres")]
        [SerializeField]
        private float rangeRadius = 5f;
        [SerializeField]
        private int count = 500;
        [SerializeField]
        private float speedMax = 2f;

        [Header("Query")]
        [SerializeField]
        private UVector2 queryCenter = UVector2.zero;
        [SerializeField]
        private float queryRadius = 3f;

        [Header("Options")]
        [SerializeField]
        private bool autoUpdate;
        [SerializeField]
        private bool drawKDTree;
        [SerializeField]
        private bool drawPoints;


        private List<PointWithState> managed = new List<PointWithState>();
        private KDTree<PointWithState> tree;
        private HashSet<PointWithState> queryResult = new HashSet<PointWithState>();

        private TimeCostRecorder buildRecorder = new TimeCostRecorder();
        private TimeCostRecorder queryRecorder = new TimeCostRecorder();


        public void BuildKDTree()
        {
            buildRecorder.Start();
            tree.Rebuild();
            buildRecorder.Stop();
        }

        private void QueryInTree()
        {
            queryRecorder.Start();
            tree.QueryInRange(queryCenter.ToAVec(), queryRadius, queryResult);
            queryRecorder.Stop();
        }


        void Start()
        {
            tree = new KDTree<PointWithState>();
            //Random.InitState(1385);
            foreach (var i in Enumerable.Range(1,count))
            {
                var v2 = Random.insideUnitCircle * rangeRadius;
                var pws = new PointWithState();
                pws.pos = v2.ToAVec();
                managed.Add(pws);
                tree.Add(pws);
            }
        }

        void Update()
        {
            if(!autoUpdate)
            {
                return;
            }

            float move = speedMax * Time.deltaTime;
            foreach(var pws in managed)
            {
                AVector2 pos = pws.pos;
                pos.x += (Random.value - 0.5f) * 2 * move;
                pos.y += (Random.value - 0.5f) * 2 * move;
                pws.pos = pos;
            }

            BuildKDTree();
            QueryInTree();
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            int frame = Time.frameCount;
            foreach(var pws in queryResult)
            {
                pws.lastUpdateFrame = frame;
            }

            if(drawPoints)
            {
                Color backup = Gizmos.color;
                foreach (var pws in managed)
                {
                    bool selected = (pws.lastUpdateFrame == frame);
                    Gizmos.color = selected ? Color.blue : Color.white;
                    Gizmos.DrawWireSphere(pws.pos.ToUVec2(), 0.02f);
                }
                Gizmos.color = backup;
            }

            if (drawKDTree)
            {
                tree.Visit(DrawKDNode);
            }
        }

        private void DrawKDNode(KDNode node)
        {
            if (node == null)
            {
                return;
            }

            if (node.leftLeaf == null && node.rightLeaf == null)
            {
                // 真leaf
                AVector2 min = node.min;
                AVector2 max = node.max;

                Vector3 p00 = new Vector3(min.x, min.y, 0);
                Vector3 p01 = new Vector3(min.x, max.y, 0);
                Vector3 p10 = new Vector3(max.x, min.y, 0);
                Vector3 p11 = new Vector3(max.x, max.y, 0);

                Gizmos.DrawLine(p00, p01);
                Gizmos.DrawLine(p01, p11);
                Gizmos.DrawLine(p11, p10);
                Gizmos.DrawLine(p10, p00);
            }
        }





#if UNITY_EDITOR
        [CustomEditor(typeof(TestKDTree))]
        public class TestKDTreeEditor : Editor
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
                TestKDTree test = target as TestKDTree;
                GUILayout.Label($"BuildAvgCost(ms) = {test.buildRecorder.GetTimeCostAvgMS()}");
                GUILayout.Label($"BuildMinCost(ms) = {test.buildRecorder.GetTimeCostMinMS()}");
                GUILayout.Label($"BuildMaxCost(ms) = {test.buildRecorder.GetTimeCostMaxMS()}");
                GUILayout.Label($"QueryAvgCost(ms) = {test.queryRecorder.GetTimeCostAvgMS()}");
                GUILayout.Label($"QueryMinCost(ms) = {test.queryRecorder.GetTimeCostMinMS()}");
                GUILayout.Label($"QueryMaxCost(ms) = {test.queryRecorder.GetTimeCostMaxMS()}");
                GUILayout.EndVertical();
            }
        }

#endif
        
    }
}