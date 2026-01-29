using System;
using TMPro;
using UnityEngine;

public class GameplayCanvasConnector : MonoBehaviour, IOnRunStart
{
    [SerializeField]
    private TextMeshProUGUI goldCountText;
    [SerializeField]
    private string goldTextPreText;
    [Space(10)]
    [SerializeField]
    private TextMeshProUGUI cleanCountText;
    [SerializeField]
    private string cleaningTextPreText;
    [Space(10)]
    [SerializeField]
    private TextMeshProUGUI timerText;
    [SerializeField]
    private GameplayRunTimer timerComp;
    [SerializeField]
    private string timerTextPreText;
    [Space(10)]
    [SerializeField]
    private TextMeshProUGUI hpText;
    [SerializeField]
    private string hpPreText;
    [SerializeField]
    private Health healthComp;
    
    public void OnRunStart()
    {
        if (!IsValid())
            return;
        var counterRef = SceneData.instance.GetRegisteredObject<ScoreCounter>();
        if (counterRef == null)
            return;
        counterRef.onGoldCountChange += value => goldCountText.text = $"{goldTextPreText} {value}";
        goldCountText.text = $"{goldTextPreText} 0";
        counterRef.onCleanCountChange += value => cleanCountText.text = $"{cleaningTextPreText} {value:0.00}";
        cleanCountText.text = $"{cleaningTextPreText} 0.00";
        timerComp.onPlaying = value => timerText.text = $"{timerTextPreText} {value}";
        
        healthComp.OnHit.AddListener(value => hpText.text = $"{hpPreText} {healthComp}");
        hpText.text = $"{hpPreText} {healthComp}";
    }

    /// <summary>
    /// Checks if this GameplayCanvasConnector is valid
    /// </summary>
    /// <returns></returns>
    private bool IsValid() => goldCountText && cleanCountText && timerText && timerComp && hpText && healthComp;

    private void OnValidate()
    {
        if (goldCountText) goldCountText.text = $"{goldTextPreText} 999";
        if (cleanCountText) cleanCountText.text = $"{cleaningTextPreText} 9999";
        if (timerText) timerText.text = $"{timerTextPreText} 00:00";
        if (hpText && healthComp) hpText.text = $"{hpPreText} {healthComp}";
    }
}
