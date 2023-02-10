using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Chunk))]
public class ChunkEditor : Editor {
    public override void OnInspectorGUI() 
    {
        base.OnInspectorGUI();
        Chunk chunk = (Chunk)(target);

        if(GUILayout.Button("Generate Chunk"))
        {
            chunk.CreateChunk();
        }
        
    }
}