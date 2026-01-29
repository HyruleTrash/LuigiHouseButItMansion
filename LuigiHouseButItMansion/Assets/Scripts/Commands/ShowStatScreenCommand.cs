using UnityEngine.SceneManagement;

public class ShowStatScreenCommand : ICommand
{
    public void Execute()
    {
        SceneManager.LoadScene("StatScreenMenu");
    }
}