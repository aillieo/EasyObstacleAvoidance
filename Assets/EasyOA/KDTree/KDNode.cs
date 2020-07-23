namespace AillieoUtils
{
    public class KDNode
    {
        public KDNode leftLeaf;
        public KDNode rightLeaf;
        
        public int startIndex;
        public int endIndex;

        public Vector2 min;
        public Vector2 max;

        public Vector2.Axis splitAxis;
        public float splitPos;
    }
}
