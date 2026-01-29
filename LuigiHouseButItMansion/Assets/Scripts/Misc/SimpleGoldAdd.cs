using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class SimpleGoldAdd : MonoBehaviour
{
    [SerializeField]
    private Vector2Int minMaxReward;
    [SerializeField]
    private GameObject cashEffectPrefab;
    [SerializeField]
    private Quaternion direction;
    [SerializeField]
    private InteractableObject interactableObject;

    private void OnValidate()
    {
        if (!interactableObject) interactableObject = GetComponent<InteractableObject>();
        enabled = interactableObject && cashEffectPrefab;
    }

    public void Trigger()
    {
        var instance = Instantiate(cashEffectPrefab,
            interactableObject.GetSpawnPoint(),
            (direction * transform.rotation),
            transform);
        instance.GetComponent<ParticleSystem>()?.Play();
        var scoreCounter = SceneData.instance.GetRegisteredObject<ScoreCounter>();
        if (scoreCounter != null) scoreCounter.GoldCount += Random.Range(minMaxReward.x, minMaxReward.y);
        Destroy(this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        var origin = interactableObject.GetSpawnPoint();
        Gizmos.DrawLine(origin, origin + (direction * transform.forward));
    }
}
