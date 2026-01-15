using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;

#if UNITY_EDITOR
public static class SceneViewShortcuts
{
    [Shortcut("Scene View Camera - Top view", KeyCode.Keypad8)]
    public static void TopView()
    {
        MakeSceneViewCameraLookAtPivot(Quaternion.Euler(90, 0, 0));
    }

    [Shortcut("Scene View Camera - Bottom view", KeyCode.Keypad2)]
    public static void BottomView()
    {
        MakeSceneViewCameraLookAtPivot(Quaternion.Euler(-90, 0, 0));
    }

    [Shortcut("Scene View Camera - Right view", KeyCode.Keypad6)]
    public static void RightView()
    {
        MakeSceneViewCameraLookAtPivot(Quaternion.Euler(0, -90, 0));
    }

    [Shortcut("Scene View Camera - Left view", KeyCode.Keypad4)]
    public static void LeftView()
    {
        MakeSceneViewCameraLookAtPivot(Quaternion.Euler(0, 90, 0));
    }

    [Shortcut("Scene View Camera - Front view", KeyCode.Keypad7)]
    public static void FrontView()
    {
        MakeSceneViewCameraLookAtPivot(Quaternion.Euler(0, 0, 0));
    }

    [Shortcut("Scene View Camera - Back view", KeyCode.Keypad1)]
    public static void BackView()
    {
        MakeSceneViewCameraLookAtPivot(Quaternion.Euler(0, 180, 0));
    }

    [Shortcut("Scene View Camera - Toggle ortho/persp view", KeyCode.Keypad5)]
    public static void ToggleOrthoPerspView()
    {
        SceneView sceneView = SceneView.lastActiveSceneView;

        if (sceneView == null) return;

        sceneView.orthographic = !sceneView.orthographic;
    }

    private static void MakeSceneViewCameraLookAtPivot(Quaternion direction)
    {
        SceneView sceneView = SceneView.lastActiveSceneView;

        if (sceneView == null) return;

        Camera camera = sceneView.camera;
        Vector3 pivot = camera.transform.position + camera.transform.forward * sceneView.cameraDistance;
        sceneView.LookAt(pivot, direction);
    }
}
#endif