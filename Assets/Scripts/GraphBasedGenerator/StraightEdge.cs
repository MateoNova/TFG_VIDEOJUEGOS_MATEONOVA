using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace GraphBasedGenerator
{
    public class StraightEdge : Edge
    {
        //change the dege to always be straight
        public override bool UpdateEdgeControl()
        {
            if (edgeControl == null || output == null || input == null)
                return false;
            
            var start = output.GetGlobalCenter();
            var end = input.GetGlobalCenter();
            var startTangent = start + Vector3.right * 50;
            var endTangent = end + Vector3.left * 50;
            edgeControl.from= new Vector2(startTangent.sqrMagnitude, endTangent.sqrMagnitude);
            return true;
        }
    }
}