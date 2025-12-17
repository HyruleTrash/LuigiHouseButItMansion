using UnityEngine;

public class ExitCommand : ICommand
{
    public void Execute()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}