using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using Random = System.Random;

public class LightFlickering : MonoBehaviour
{
    private const float Delay = -0.01f;
    private readonly Random rnd = new Random();

    [SerializeField] private float minTime = 0.1f;
    [SerializeField] private float maxTime = 0.5f;

    private Light flickeringLight;
    private float deltaTime;

    public float NextDuration
    {
        get { return (float) rnd.NextDouble() * deltaTime + minTime; }
    }

    [UsedImplicitly]
    private void Awake()
    {
        deltaTime = maxTime - minTime;
        flickeringLight = GetComponentInChildren<Light>(false);
    }

    public float Flicker(int count)
    {
        var flickers = GetFlickeringIntervals(count).ToList();
        StartCoroutine(FlickerCoroutine(flickers));
        return flickers.Sum();
    }

    private IEnumerable<float> GetFlickeringIntervals(int count)
    {
        var flickeringCount = count * 2 - 1;
        for (var i = 0; i < flickeringCount; i++)
            yield return NextDuration;
        yield return Delay;
    }

    private IEnumerator FlickerCoroutine(IEnumerable<float> flickerIntervals)
    {
        foreach (var interval in flickerIntervals)
        {
            flickeringLight.enabled = !flickeringLight.enabled;
            yield return new WaitForSeconds(interval);
        }
    }
}