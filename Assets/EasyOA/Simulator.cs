using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AillieoUtils
{
    public class Simulator
    {
        public readonly Config config;

        private readonly KDTree<Agent> kDTree = new KDTree<Agent>();
        private readonly List<Agent> managedAgents = new List<Agent>();
        private readonly Dictionary<int, int> indexById = new Dictionary<int, int>();
        private int sid = 0;
        private bool indexDirty = false; // todo改成一个int表示list里第几个开始失效
        private readonly long[] failureRecorder = new long[2];

        public Simulator(Config config = null)
        {
            if(config == null)
            {
                config = Config.DefaultConfig;
            }
            this.config = config;
            kDTree.SetLeafSizeMax(config.leafSizeMax);
        }

        public Agent CreateAgent()
        {
            Agent agent = new Agent(sid++);
            managedAgents.Add(agent);
            int index = managedAgents.Count;
            indexById.Add(agent.id, index);
            return agent;
        }

        public Agent GetAgent(int id)
        {
            int index = GetIndex(id);
            if (index >= 0)
            {
                return managedAgents[index];
            }
            return null;
        }

        public bool RemoveAgent(int id)
        {
            int index = GetIndex(id);
            if (index >= 0)
            {
                managedAgents.RemoveAt(index);
                indexById.Remove(id);
                indexDirty = true;
                return true;
            }
            return false;
        }

        private int GetIndex(int id)
        {
            if (indexDirty)
            {
                ReIndex();
                indexDirty = false;
            }

            int index;
            if (indexById.TryGetValue(id, out index))
            {
                return index;
            }
            return -1;
        }

        private void ReIndex()
        {
            indexById.Clear();
            for (int i = 0, len = managedAgents.Count; i < len; ++i)
            {
                indexById.Add(managedAgents[i].id, i);
            }
        }

        public void Step(float deltaTime)
        {
            // 1. kdtree同步
            // 优化：增加脏标记
            kDTree.Clear();
            kDTree.Add(managedAgents);

            // 2. 分裂成子迭代步
            int subSteps = Math.Max(config.subSteps, 1);
            for (int i = 0; i < subSteps; ++i)
            {
                OASubStep(deltaTime / subSteps);
            }

            // 3. 检查更新失败率
            if(config.failureRecording)
            {
                float sqConflict = (1 - config.conflictTolerance) * (1 - config.conflictTolerance);
                foreach (var agent in managedAgents)
                {
                    foreach (var other in managedAgents)
                    {
                        if (agent == other) { continue; }
                        float distSq = (agent.position - other.position).sqrMagnitude;
                        float radiusSum = agent.radius + other.radius;
                        failureRecorder[0]++;
                        if (distSq < radiusSum * radiusSum * sqConflict)
                        {
                            failureRecorder[1]++;
                        }
                    }
                }
            }
        }

        private void OASubStep(float deltaTime)
        {
            // 1. 初始化
            foreach (var agent in managedAgents)
            {
                agent.moveWithOA = agent.goal - agent.position;
            }

            // 2. 进入迭代步
            int totalSteps = 1 + config.fixingSteps;
            for (int i = 0; i < totalSteps; ++i)
            {
                kDTree.Rebuild();

                // 2.1 从kdtree获取neighbor
                foreach (var agent in managedAgents)
                {
                    kDTree.QueryInRange(agent.position, config.neighborFactor, agent.neighbors);
                }

                // 2.2 避让速度修正
                foreach (var agent in managedAgents)
                {
                    OA(agent, deltaTime);
                }
            }

            // 3. 根据速度移动单位
            foreach (var agent in managedAgents)
            {
                MoveAgent(agent, deltaTime);
            }
        }

        private void OA(Agent agent, float timeStep)
        {
            agent.collisions.Clear();

            Vector2 expected = agent.moveWithOA;
            Vector2 moveWithOADir = expected;
            Vector2 rightHand = Vector2.Perpendicular(expected);

            float avoidLeft = 0;
            float avoidRight = 0;

            foreach (var neighbor in agent.neighbors)
            {
                if (neighbor != agent)
                {
                    Vector2 posRelative = agent.position - neighbor.position;
                    if (Vector2.Dot(expected, - posRelative) > 0)
                    {
                        float distanceSq = posRelative.SqrMagnitude();
                        float minDistAllow = (agent.radius + neighbor.radius) * (1 + config.spaceFactor);

                        float distanceIgnore = agent.speed * timeStep * config.distanceIgnoreFactor + minDistAllow;
                        if(distanceSq > distanceIgnore * distanceIgnore)
                        {
                            continue;
                        }

                        // 在垂直方向上（右手边）的投影
                        float distHorizontal = Vector2.Dot(posRelative, rightHand);

                        // 方向会冲突 && 距离足够近 （向前和向右的投影 都小于半径之和）
                        if (Vector2.Dot(posRelative, moveWithOADir) < minDistAllow && Math.Abs(distHorizontal) < minDistAllow)
                        {
                            //if (Math.Abs(distHorizontal) <= float.Epsilon)
                            //{
                            //    Debug.LogError("正前方");
                            //}

                            float dist = (float)Math.Sqrt(distanceSq);
                            float hFactor = config.horizontalFactor;

                            if (distHorizontal > 0)
                            {
                                avoidRight += (minDistAllow - distHorizontal) * hFactor * dist;
                                moveWithOADir += avoidRight * rightHand;
                                moveWithOADir.Normalize();
                            }
                            else
                            {
                                avoidLeft += (minDistAllow + distHorizontal) * hFactor * dist;
                                moveWithOADir -= avoidLeft * rightHand;
                                moveWithOADir.Normalize();
                            }

                            rightHand = Vector2.Perpendicular(moveWithOADir);

                            agent.collisions.Add(neighbor);
                        }
                    }
                }
            }

            var oa = Vector2.Perpendicular(expected) * (avoidRight - avoidLeft);

            moveWithOADir = (expected + oa).normalized;

            float mag = Math.Min(agent.speed * timeStep, Vector2.Distance(agent.goal, agent.position));
            agent.moveWithOA = moveWithOADir * mag;
        }


        private static void MoveAgent(Agent agent, float timeStep)
        {

            foreach (var c in agent.collisions)
            {
                // 如果按照move移动之后 会碰撞 那么截断
                Vector2 relativeMove = agent.moveWithOA - c.moveWithOA;
                Vector2 relativePos = agent.position - c.position;
                float relativeDist = relativePos.magnitude - agent.radius - c.radius;
                relativePos.Normalize();
                relativePos *= relativeDist;
                float project = Vector2.Dot(relativePos, relativeMove);
                if (project > 0)
                {
                    //Debug.LogError("collision");
                    agent.moveWithOA = agent.moveWithOA.normalized * Math.Min(agent.moveWithOA.magnitude, Math.Abs(project));
                }
            }
            agent.position += agent.moveWithOA;
        }

        public float GetFailureRate()
        {
            if(failureRecorder[0] == 0)
            {
                return 0;
            }
            return (float)failureRecorder[1] / failureRecorder[0];
        }
    }
}
