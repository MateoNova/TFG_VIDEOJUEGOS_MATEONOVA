using UnityEngine;

namespace GraphBasedGenerator
{
    [OpenGraphEditor]
    public class GraphBasedGenerator : BaseGenerator
    {
         public bool closeGraphEditorAutomatically = true;

        public override void RunGeneration(bool resetTilemap = true, Vector2Int startPoint = default)
        {
            throw new System.NotImplementedException();
        }
        
        public override void OpenGraphWindow()
        {
            GraphWindow.ShowWindow();
        }
    }
}