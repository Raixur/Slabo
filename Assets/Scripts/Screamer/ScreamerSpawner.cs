using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ScreamerSpawner : MonoBehaviour
{
    private const float Angle = 10f;

    [SerializeField] private List<Screamer> screamers;
    [SerializeField] private List<LightFlickering> affectedLight;
    [SerializeField] private Transform pointingObject;

    private Queue<Screamer> screamerQueue;

    private bool isActive;

    public void Start()
    {
        pointingObject = pointingObject ?? VRTK_SDK_Bridge.GetHeadsetCamera();

        foreach (var screamer in screamers)
        {
            screamer.Spawn += (sender, args) =>
            {
                affectedLight.ForEach(l => l.FlickerByTime(args.Duration));
                isActive = true;
            };
            screamer.Despawn += (sender, args) => isActive = false;
        }

        screamerQueue = new Queue<Screamer>(screamers);
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<VRTK_PlayerObject>() != null && IsPointedOnScreamer() && !isActive)
        {
            var screamer = screamerQueue.Dequeue();
            screamer.InitSpawn();
        }
    }

    private bool IsPointedOnScreamer()
    {
        var destDir = screamerQueue.Peek().transform.position - pointingObject.transform.position; 
        return Vector3.Angle(destDir, pointingObject.transform.forward) < Angle;
    }
}
