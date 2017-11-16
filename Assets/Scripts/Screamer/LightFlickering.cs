using System.Collections;
using UnityEngine;
using Random = System.Random;

public class LightFlickering : MonoBehaviour
{
    private readonly Random rnd = new Random();

    [SerializeField] private float minFadeTime = 0.1f;
    [SerializeField] private float maxFadeTime = 0.5f;

    [SerializeField] private float minUnfadeTime = 0.1f;
    [SerializeField] private float maxUnfadeTime = 0.5f;

    [SerializeField] private Light flickeringLight = null;

    public void StartFlickering(int count)
    {
        // Disable
        StartCoroutine(StartToggle(count));
    }

    private IEnumerator StartToggle(int count)
    {
        var deltaFadeTime = maxFadeTime - minFadeTime;
        var deltaUnfadeTime = maxUnfadeTime - minUnfadeTime;

        for (var i = 0; i < count; i++)
        {
            flickeringLight.enabled = false;
            yield return new WaitForSeconds((float) rnd.NextDouble() * deltaFadeTime + minFadeTime);

            flickeringLight.enabled = true;
            yield return new WaitForSeconds((float)rnd.NextDouble() * deltaUnfadeTime + minUnfadeTime);
        }
    }
}
