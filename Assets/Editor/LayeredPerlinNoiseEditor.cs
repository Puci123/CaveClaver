using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LayeredPerlinNoise))]
public class LayeredPerlinNoiseEditor : Editor 
{
    public override void OnInspectorGUI() 
    {
         base.OnInspectorGUI();

        LayeredPerlinNoise noise = (LayeredPerlinNoise)target;
        
      
        if(GUILayout.Button("Create Texture"))
        {
            noise.CreateTexture(1024, 1024);
        }
        
        if(noise.texture != null)
        {
           // EditorGUI.DrawPreviewTexture(new Rect(110, 200, 200, 200), noise.texture);
        }

        
    }
}