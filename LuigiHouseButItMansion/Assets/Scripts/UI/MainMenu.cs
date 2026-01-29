using System;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public RoomObjectData menuRoomData;

    private void Start()
    {
        if(menuRoomData == null)
            return;
        menuRoomData.goopManager.SetGlobalShaderData();
    }

    public void PlayGame()
    {
        var playCmd = new PlayCommand();
        playCmd.Execute();
    }

    public void ShowOptions()
    {
        Debug.Log("Show Options unimplemented");
    }

    public void QuitGame()
    {
        var exitCmd = new ExitCommand();
        exitCmd.Execute();
    }
}
