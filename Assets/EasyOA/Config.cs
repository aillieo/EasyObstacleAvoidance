using System;
using System.Collections;
using System.Collections.Generic;

namespace AillieoUtils
{
    [Serializable]
    public class Config
    {
        public static readonly Config DefaultConfig = new Config();

        // 每次迭代分成几个子迭代步
        public int subSteps = 2;

        // 修正迭代
        public int fixingSteps = 2;

        // kdtree叶节点允许的最大单位个数
        public int leafSizeMax = 10;

        // 避让时水平方向偏移系数
        public float horizontalFactor = 18f;

        // 忽略范围以外的可能碰撞
        public float distanceIgnoreFactor = 2f;

        // 获取周围多大范围内的单位认为是相邻单位参与OA
        public float neighborFactor = 8.5f;

        // 两个单位之间的预留的空隙系数 0表示不预留
        public float spaceFactor = 0.01f;

        // 是否记录失效
        public bool failureRecording = false;

        // 当两个单位重叠超出这个数值时 认为是失效
        public float conflictTolerance = 0.01f;

    }
}
