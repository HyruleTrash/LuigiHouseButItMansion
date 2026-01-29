using JetBrains.Annotations;
using LucasCustomClasses;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitCommand : ICommand
{
    private static bool allowedToTrigger;
    [CanBeNull] private static TimerComp timer;
    [CanBeNull] private static GameObject timerObject;
    
    public void Execute()
    {
        if (!allowedToTrigger)
        {
            if (timer == null || timerObject == null)
                allowedToTrigger = true;
            else
                return;
        }
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }
        
        allowedToTrigger = false;
        
        timerObject ??= new GameObject("ExitCmdTimer");
        timer ??= timerObject.AddComponent<TimerComp>();
        if (timer == null) return;
        
        Object.DontDestroyOnLoad(timerObject);
        timer.timer = new Timer(0.5f, () =>
        {
            allowedToTrigger = true;
            timer.gameObject.SetActive(false);
        })
        {
            running = true
        };
    }
}