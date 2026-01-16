
using System;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(fileName = "GoopData", menuName = "ScriptableObjects/RoomData/Goop")]
public class GoopData : ScriptableObject
{
    private static readonly int GoopTex = Shader.PropertyToID("_GoopTex");
    private static readonly int GoopTiling = Shader.PropertyToID("_GoopTiling");
    private static readonly int GoopColor = Shader.PropertyToID("_GoopColor");
    private static readonly int GoopAccentColor = Shader.PropertyToID("_GoopAccentColor");
    private static readonly int CutOffThreshold = Shader.PropertyToID("_CutOffThreshold");
    
    public Texture2D goopTexture;
    public float goopTiling;
    public Color goopColor;
    public Color goopAccentColor;
    public float cutoffThreshold = 0.1f;
    public int textureSize = 64;
    public int brushSize = 5;

    public void SetGlobalShaderData()
    {
        Shader.SetGlobalColor(GoopColor, goopColor);
        Shader.SetGlobalColor(GoopAccentColor, goopAccentColor);
        Shader.SetGlobalFloat(CutOffThreshold, cutoffThreshold);
        Shader.SetGlobalTexture(GoopTex, goopTexture);
        Shader.SetGlobalFloat(GoopTiling, goopTiling);
    }
}