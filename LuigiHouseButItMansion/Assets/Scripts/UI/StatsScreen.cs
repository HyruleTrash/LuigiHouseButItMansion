using System;
using TMPro;
using UnityEngine;

public class StatsScreen : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI goldCountText;
    [SerializeField]
    private TextMeshProUGUI cleanedCountText;
    [SerializeField]
    private TextMeshProUGUI totalCountText;
    
    public void Start()
    {
        var counterRef = SceneData.instance.GetRegisteredObject<ScoreCounter>();
        if (counterRef == null) return;

        goldCountText.text = counterRef.GoldCount.ToString();
        cleanedCountText.text = counterRef.CleanCount.ToString("0.00");
        totalCountText.text = (counterRef.GoldCount + counterRef.CleanCount).ToString("0.00");
    }
    
    public void ReStartGame()
    {
        SceneData.instance.DeRegistereObject<ScoreCounter>();
        var playCmd = new PlayCommand();
        playCmd.Execute();
    }

    public void QuitGame()
    {
        var exitCmd = new ExitCommand();
        exitCmd.Execute();
    }
}
