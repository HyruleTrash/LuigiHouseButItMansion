using UnityEngine;
using UnityEngine.InputSystem;

public class MouseRayGetter : SingletonBehaviour<MouseRayGetter>
{
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private RenderTexture renderTexture;
    
    public Ray GetMouseRay()
    {
        float CalculateMousePos(float mPos, float sSize, int rTex) => 1f / sSize * mPos * rTex;
        
        var mousePos = Mouse.current.position.ReadValue();
        var trueMousePos = new Vector2(CalculateMousePos(mousePos.x, Screen.width, renderTexture.width),
            CalculateMousePos(mousePos.y, Screen.height, renderTexture.height));
        
        return cam.ScreenPointToRay(trueMousePos);
    }
}