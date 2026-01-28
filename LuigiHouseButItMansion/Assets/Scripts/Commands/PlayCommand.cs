using UnityEngine.SceneManagement;

public class PlayCommand : ICommand
{
    public void Execute()
    {
        SceneManager.LoadScene("MainScene");
    }
}