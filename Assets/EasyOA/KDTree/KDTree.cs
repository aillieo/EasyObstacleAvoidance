using System;
using System.Collections.Generic;

namespace AillieoUtils
{
    public class KDTree<T> where T: IPositionProvider
    {
        public class NodePool
        {
            private int index = 0;
            private readonly List<KDNode> nodes = new List<KDNode>();

            public KDNode GetNew()
            {
                if (index >= nodes.Count)
                {
                    nodes.Add(new KDNode());
                }
                return nodes[index++];
            }
            
            public void Reset()
            {
                index = 0;
            }
        }

        private int leafSizeMax = 10;
        private readonly NodePool nodePool = new NodePool();
        private readonly List<T> managed = new List<T>();
        private readonly List<int> permutation = new List<int>();
        private KDNode root;
        private readonly Queue<KDNode> processingQueue = new Queue<KDNode>();

        public void Add(T ipp)
        {
            managed.Add(ipp);
        }

        public void Add(IEnumerable<T> ipps)
        {
            managed.AddRange(ipps);
        }

        public void Clear()
        {
            managed.Clear();
        }

        public void SetLeafSizeMax(int newSize)
        {
            leafSizeMax = Math.Max(1, newSize);
        }
        
        public void Rebuild()
        {
            if(managed.Count == 0)
            {
                return;
            }

            int count = managed.Count;
            for(int i = 0; i < count; i++) {
                if (permutation.Count <= i)
                {
                    permutation.Add(i);
                }
                else
                {
                    permutation[i] = i;
                }
            }

            nodePool.Reset();

            root = nodePool.GetNew();
            root.startIndex = 0;
            root.endIndex = count;
            FindBounds(out root.min, out root.max);

            
            processingQueue.Clear();
            processingQueue.Enqueue(root);

            while (processingQueue.Count > 0)
            {
                KDNode node = processingQueue.Dequeue();
                Split(node);
            }
        }

        private void FindBounds(out Vector2 min, out Vector2 max)
        {
            // todo 效率优化
            min = managed[0].position;
            max = managed[0].position;
            foreach (var pp in managed)
            {
                min = Vector2.Min(min, pp.position);
                max = Vector2.Max(max, pp.position);
            }
        }

        private void Split(KDNode node)
        {
            node.leftLeaf = null;
            node.rightLeaf = null;

            if (node.endIndex - node.startIndex <= leafSizeMax)
            {
                return;
            }

            FindSplitAxis(node);
            int splittingIndex = FindSplitIndex(node);

            //Debug.Log($"start={node.startIndex} end={node.endIndex} split={splittingIndex}");

            KDNode leftLeaf = nodePool.GetNew();
            leftLeaf.min = node.min;
            Vector2 leftMax = node.max;
            leftMax[node.splitAxis] = node.splitPos;
            leftLeaf.max = leftMax;
            leftLeaf.startIndex = node.startIndex;
            leftLeaf.endIndex = splittingIndex;
            node.leftLeaf = leftLeaf;
            
            KDNode rightLeaf = nodePool.GetNew();
            rightLeaf.max = node.max;
            Vector2 rightMin = node.min;
            rightMin[node.splitAxis] = node.splitPos;
            rightLeaf.min = rightMin;
            rightLeaf.startIndex = splittingIndex;
            rightLeaf.endIndex = node.endIndex;
            node.rightLeaf = rightLeaf;
            
            processingQueue.Enqueue(leftLeaf);
            processingQueue.Enqueue(rightLeaf);
        }

        private void FindSplitAxis(KDNode node)
        {
            // 比较快的方法 选择范围大的维度
            Vector2 range = node.max - node.min;
            Vector2.Axis splitAxis = Vector2.Axis.X;
            if (range.y > range.x)
            {
                splitAxis = Vector2.Axis.Y;
            }

            node.splitAxis = splitAxis;
        }
        
        private int FindSplitIndex(KDNode node, int retryOffset = 0)
        {
            // 取第一个 并将其后移 最终保证 左边都比它小 右边都比它大 有点类似于快速排序第一步
            int start = node.startIndex + retryOffset;
            int end = node.endIndex - 1;
            Vector2.Axis splitAxis = node.splitAxis;
            int pivot = permutation[start];
            while (start < end)
            {
                while (start < end && managed[permutation[end]].position[splitAxis] >= managed[pivot].position[splitAxis])
                {
                    --end;
                }
                permutation[start] = permutation[end];
                while (start < end && managed[permutation[start]].position[splitAxis] <= managed[pivot].position[splitAxis])
                {
                    ++start;
                }
                permutation[end] = permutation[start];
            }
            permutation[start] = pivot;
            
            int foundIndex = start;
            
            if (foundIndex == node.startIndex || foundIndex == node.endIndex)
            {
                // 运气不太好 出现了极限情况
                if (node.startIndex + retryOffset < node.endIndex - 1)
                {
                    // Debug.LogError($"再试一次    start={node.startIndex}  off={retryOffset}  end={node.endIndex}");
                    foundIndex = FindSplitIndex(node, retryOffset + 1);
                    return foundIndex;
                }
            }
            
            node.splitPos = managed[permutation[foundIndex]].position[splitAxis];
            return foundIndex;
        }
        
        public IEnumerable<T> QueryInRange(Vector2 center, float radius)
        {
            List<T> list = new List<T>();
            QueryInRange(center, radius, list);
            return list;
        }

        public void QueryInRange(Vector2 center, float radius, ICollection<T> toFill)
        {
            if (managed.Count == 0)
            {
                return;
            }

            processingQueue.Clear();
            toFill.Clear();

            float radiusSq = radius * radius;
            
            processingQueue.Enqueue(root);

            while (processingQueue.Count > 0)
            {
                KDNode node = processingQueue.Dequeue();
                if (node.leftLeaf == null && node.rightLeaf == null)
                {
                    // leaf
                    for (int i = node.startIndex; i < node.endIndex; ++i)
                    {
                        int index = permutation[i];
                        if (Vector2.SqrMagnitude(managed[index].position - center) <= radiusSq)
                        {
                            toFill.Add(managed[index]);
                        }
                    }
                }
                else
                {
                    // todo 可以缓存更多信息 加速叶节点的查找
                    if (IsKDNodeInRange(node.leftLeaf, center, radiusSq))
                    {
                        processingQueue.Enqueue(node.leftLeaf);
                    }
                    if (IsKDNodeInRange(node.rightLeaf, center, radiusSq))
                    {
                        processingQueue.Enqueue(node.rightLeaf);
                    }
                }
            }
        }

        private bool IsKDNodeInRange(KDNode node, Vector2 center, float radiusSq)
        {
            Vector2 nodeCenter = (node.max + node.min) * 0.5f;
            Vector2 nodeRange = node.max - node.min;
            Vector2 v = Vector2.Max(nodeCenter - center, -(nodeCenter - center));
            Vector2 u = Vector2.Max(v - nodeRange,Vector2.zero);
            return u.sqrMagnitude < radiusSq;
        }
        
        public void Visit(Action<KDNode> func)
        {
            if(func == null)
            {
                return;
            }
            VisitNode(func, root);
        }

        private void VisitNode(Action<KDNode> func, KDNode node)
        {
            if(node == null)
            {
                return;
            }

            func(node);

            if(node.leftLeaf != null && node.rightLeaf != null)
            {
                VisitNode(func, node.leftLeaf);
                VisitNode(func, node.rightLeaf);
            }
        }

    }
}
