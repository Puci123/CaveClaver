using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Noise/Layered Perlin Noise")]
public class LayeredPerlinNoise : ScriptableObject
{
    [SerializeField,Range(1,10)] private float _lacunarity = 2f;    
    [SerializeField, Range(0,1)] private float _persistance = 0.5f;
    [SerializeField, Range(1,10)] private int _octaves = 4;

    [SerializeField, Range(-1,1)] private float _bias = 0.1f;
    [SerializeField] private Vector2 offset = new Vector2(201.312321f, 132.124214f);
    [SerializeField] private float _scale = 2;
    [HideInInspector] public Texture2D texture;

    public float GetNoiseValue(float x, float y)
    {   
        float value = 0;
        float frequency = 1;
        float amplitude = 1;

        x += offset.x;
        y += offset.y;

        for (int i = 0; i < _octaves; i++)
        {
            value += Mathf.PerlinNoise((x / _scale) * frequency, (y / _scale) * frequency) * amplitude; 
            frequency *= _lacunarity;
            amplitude *= _persistance;
        }

        return Mathf.Clamp01(value + _bias);
    }

    public void CreateTexture(int w, int h)
    {
        texture = new Texture2D(w,h);
        
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                float sample = GetNoiseValue(x / 40,y / 40);
                texture.SetPixel(x,y,new Color(sample,sample,sample));
            }   
        }

        texture.Apply();
    }
    

}
