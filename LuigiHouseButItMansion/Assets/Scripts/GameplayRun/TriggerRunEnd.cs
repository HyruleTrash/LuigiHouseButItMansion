using UnityEngine;

public class TriggerRunEnd : MonoBehaviour
{
    public void TriggerEnd()
    {
        var cmd = new ShowStatScreenCommand();
        cmd.Execute();
    }
}
