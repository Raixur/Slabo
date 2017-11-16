using System.Collections;
using UnityEngine;
using Random = System.Random;

[RequireComponent(typeof(LightFlickering))]
public class RandomLightFlickering : MonoBehaviour
{
    private readonly Random rnd = new Random();

    [SerializeField] private float maxTime = 240f;
    [SerializeField] private float minTime = 30f;

    [SerializeField] private int minFlick = 1;
    [SerializeField] private int maxFlick = 4;

    private LightFlickering lightFlickering;

    public void Start()
    {
        lightFlickering = GetComponent<LightFlickering>();
        StartCoroutine(StartFlickering());
    }

    private IEnumerator StartFlickering()
    {
        var dTime = maxTime - minTime;
        while (true)
        {
            yield return new WaitForSeconds((float) rnd.NextDouble() * dTime + minTime);
            lightFlickering.FlickerByCount(rnd.Next(minFlick, maxFlick));
        }
    }
}
