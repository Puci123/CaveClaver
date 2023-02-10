using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChunkManager))]
public class ChunkManagerEditor : Editor {
    public override void OnInspectorGUI() 
    {
        base.OnInspectorGUI();
        ChunkManager chunkManager = (ChunkManager)target;

        DrawOptions(chunkManager.Noise,ref chunkManager.NoiseOptionsFoldOut);
        

        if(GUILayout.Button("Create mother chunk"))
        {
            chunkManager.ClearData();
            chunkManager.CreateChunk(Vector3.zero,Vector2Int.zero);
        }

        if(GUILayout.Button("Update visible chunks"))
        {
            chunkManager.ClearData();
            chunkManager.UpdateVisiableChunks();
        }

        if(GUILayout.Button("Clear Data"))
        {
            chunkManager.ClearData();
        }

    }

    void DrawOptions(Object opt, ref bool foldout)
    {
        if(opt != null)
        {
            foldout =  EditorGUILayout.InspectorTitlebar(foldout,opt);
            if(foldout)
            {
                using(var check = new EditorGUI.ChangeCheckScope())
                {
                    Editor editor = CreateEditor(opt);
                    editor.OnInspectorGUI();
                }
            }
        }
    }
}