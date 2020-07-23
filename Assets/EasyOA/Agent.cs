using System.Collections;
using System.Collections.Generic;
namespace AillieoUtils
{
    public class Agent : IPositionProvider
    {
        internal Agent(int id)
        {
            this.id = id;
        }
        public readonly int id;
        public float radius;
        public Vector2 goal;
        public float speed;

        public Vector2 position { get; set; }

        internal readonly List<Agent> neighbors = new List<Agent>();
        internal readonly List<Agent> collisions = new List<Agent>();
        internal Vector2 moveWithOA;
    }
}
