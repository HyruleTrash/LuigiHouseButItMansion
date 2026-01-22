#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomEditor(typeof(GoopManager))]
[CanEditMultipleObjects]
public class GoopManagerEditor : Editor
{
    // private UnityEditor.Texture3DPreview test;
    private Texture3DPreview roomTex3DPreview;
    private Texture3DPreview usedRoomTex3DPreview;
    private bool state = false;
    private float opacity = 0.8f;
    public override bool HasPreviewGUI() => targets.Length == 1;

    private void OnEnable()
    {
        roomTex3DPreview ??= new Texture3DPreview();
        usedRoomTex3DPreview ??= new Texture3DPreview();
    }
    
    private void OnDisable()
    {
        roomTex3DPreview?.Dispose();
        roomTex3DPreview = null;

        usedRoomTex3DPreview?.Dispose();
        usedRoomTex3DPreview = null;
    }


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        var targetObjects = serializedObject.targetObjects;
        if (targetObjects.Length > 1)
        {
            if (GUILayout.Button("Regenerate Texture"))
            {
                foreach (var t in targetObjects)
                {
                    var targetedManager = (GoopManager)t;
                    targetedManager.Regenerate();
                }
            }
    
            if (GUILayout.Button("Set to current"))
            {
                foreach (var t in targetObjects)
                {
                    var targetedManager = (GoopManager)t;
                    targetedManager.SetToCurrent();
                }
            } 
            return;
        }
        
        var manager = (GoopManager)serializedObject.targetObject;
        if (!manager.enabled)
            return;
        
        if (GUILayout.Button("Regenerate Texture")) manager.Regenerate();
        
        if (GUILayout.Button("Set to current")) manager.SetToCurrent();
    }
    
    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        GoopManager manager = (GoopManager)serializedObject.targetObject;
        if (!manager.enabled)
            return;
        
        const int topOffset = 22;
        const int minHeight = 16;
        string currentState = !state ? "Active Tex" : "Saved Tex";
        GUI.Label(new Rect(r.x + 5, topOffset, r.width, minHeight), currentState + ": preview");
        
        const int buttonWidth = 100;
        if (GUI.Button(new Rect(r.x + r.width - buttonWidth, topOffset-1, buttonWidth, minHeight), 
                $"Toggle preview")) state = !state;
        
        const int sliderWidth = 100;
        opacity = GUI.HorizontalSlider(
            new Rect(r.x + r.width - buttonWidth - sliderWidth - 5, topOffset - 1, sliderWidth, minHeight),
            opacity, 0, 1);

        Rect usedRect = new Rect(r.x, r.y + minHeight, r.width, r.height - minHeight);
        if (state)
        {
            if (roomTex3DPreview != null) roomTex3DPreview.opacity = opacity;
            roomTex3DPreview?.SetTexture(manager.roomTexture);
            roomTex3DPreview?.OnPreviewGUI(usedRect, background);
        }
        else
        {
            if (usedRoomTex3DPreview != null) usedRoomTex3DPreview.opacity = opacity;
            usedRoomTex3DPreview?.SetTexture(manager.usedRoomTexture);
            usedRoomTex3DPreview?.OnPreviewGUI(usedRect, background);
        }
    }
}

public class Texture3DPreview
{
    static readonly int _GlobalScale = Shader.PropertyToID("_GlobalScale");
    private static int sliderHash = "Slider".GetHashCode();
    PreviewRenderUtility preview;
    Texture3D texture;
    Vector2 previewDir;
    float viewDistance = 2.5f;

    Material volumeMat;
    public float opacity = 0.8f;

    static readonly int _VoxelSize = Shader.PropertyToID("_VoxelSize");
    static readonly int _InvResolution = Shader.PropertyToID("_InvResolution");
    static readonly int _Quality = Shader.PropertyToID("_Quality");
    static readonly int _Alpha = Shader.PropertyToID("_Alpha");
    static readonly int _CamToW = Shader.PropertyToID("_CamToW");
    static readonly int _WToCam = Shader.PropertyToID("_WToCam");
    static readonly int _ObjToW = Shader.PropertyToID("_ObjToW");
    static readonly int _WToObj = Shader.PropertyToID("_WToObj");

    public Texture3DPreview()
    {
        preview = new PreviewRenderUtility();
        preview.camera.fieldOfView = 30f;
        preview.camera.nearClipPlane = 0.1f;
        preview.camera.farClipPlane = 20f;

        volumeMat = Object.Instantiate(
            EditorGUIUtility.LoadRequired("Previews/Preview3DVolume.mat")
        ) as Material;
    }

    public void Dispose()
    {
        preview?.Cleanup();
        preview = null;
    }

    public void SetTexture(Texture3D tex)
    {
        texture = tex;
    }

    public void OnPreviewGUI(Rect rect, GUIStyle background)
    {
        if (texture == null || !SystemInfo.supports3DTextures)
            return;

        previewDir = Drag2D(previewDir, rect);

        if (Event.current.type == EventType.ScrollWheel)
        {
            viewDistance = Mathf.Clamp(viewDistance + Event.current.delta.y * 0.05f, 0.5f, 6f);
            Event.current.Use();
        }

        if (Event.current.type != EventType.Repaint)
            return;
        
        preview.camera.aspect = rect.width / rect.height;
        preview.BeginPreview(rect, background);
        preview.camera.Render();
        DrawVolume();
        preview.EndAndDrawPreview(rect);
    }
    
    public static Vector2 Drag2D(Vector2 scrollPosition, Rect position)
    {
        int controlId = GUIUtility.GetControlID(sliderHash, FocusType.Passive);
        Event current = Event.current;
        switch (current.GetTypeForControl(controlId))
        {
            case UnityEngine.EventType.MouseDown:
                if (position.Contains(current.mousePosition) && (double) position.width > 50.0)
                {
                    GUIUtility.hotControl = controlId;
                    current.Use();
                    EditorGUIUtility.SetWantsMouseJumping(1);
                    break;
                }
                break;
            case UnityEngine.EventType.MouseUp:
                if (GUIUtility.hotControl == controlId)
                    GUIUtility.hotControl = 0;
                EditorGUIUtility.SetWantsMouseJumping(0);
                break;
            case UnityEngine.EventType.MouseDrag:
                if (GUIUtility.hotControl == controlId)
                {
                    scrollPosition -= current.delta * (current.shift ? 3f : 1f) / Mathf.Min(position.width, position.height) * 140f;
                    current.Use();
                    GUI.changed = true;
                    break;
                }
                break;
        }
        return scrollPosition;
    }

    void DrawVolume()
    {
        Vector3 res = new Vector3(texture.width, texture.height, texture.depth);
        float invRes = 1f / Mathf.Max(res.x, res.y, res.z);
        Vector3 voxelSize = res * invRes;

        Quaternion rot = Quaternion.Euler(-previewDir.y, -previewDir.x, 0f);
        preview.camera.transform.position = rot * Vector3.back * viewDistance;
        preview.camera.transform.rotation = rot;

        Matrix4x4 trs = Matrix4x4.identity;

        volumeMat.mainTexture = texture;
        volumeMat.SetVector(_VoxelSize, voxelSize);
        volumeMat.SetVector(_GlobalScale, Vector3.one);
        volumeMat.SetFloat(_InvResolution, invRes);
        volumeMat.SetFloat(_Quality, invRes * 0.5f);
        volumeMat.SetFloat(_Alpha, Mathf.Pow(Mathf.Clamp01(opacity), 3));

        volumeMat.SetMatrix(_CamToW, preview.camera.cameraToWorldMatrix);
        volumeMat.SetMatrix(_WToCam, preview.camera.worldToCameraMatrix);
        volumeMat.SetMatrix(_ObjToW, trs);
        volumeMat.SetMatrix(_WToObj, trs);

        GL.PushMatrix();
        GL.LoadProjectionMatrix(preview.camera.projectionMatrix);
        volumeMat.SetPass(0);
        Graphics.DrawProceduralNow(
            MeshTopology.Quads,
            4,
            Mathf.CeilToInt(1f / invRes * 2f)
        );
        GL.PopMatrix();
    }

}
#endif