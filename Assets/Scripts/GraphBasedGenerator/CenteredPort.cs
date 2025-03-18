using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GraphBasedGenerator
{
    // Custom port that returns the node's center as its global center.
    public class CenteredPort : Port
    {
        public CenteredPort(Orientation orientation, Direction direction, Capacity capacity, System.Type type)
            : base(orientation, direction, capacity, type)
        {
        }

        public override Vector3 GetGlobalCenter()
        {
            // If the port is attached to a GraphNode, return the center of that node's rect.
            if (this.node is GraphNode node)
            {
                Rect nodeRect = node.GetPosition();
                return new Vector2(nodeRect.center.x, nodeRect.center.y);
            }
            return base.GetGlobalCenter();
        }
    }
}