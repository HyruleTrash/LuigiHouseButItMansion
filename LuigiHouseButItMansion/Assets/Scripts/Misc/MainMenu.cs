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
        var exit = new PlayCommand();
        exit.Execute();
    }

    public void ShowOptions()
    {
        Debug.Log("Show Options unimplemented");
    }

    public void QuitGame()
    {
        var exit = new ExitCommand();
        exit.Execute();
    }
}
