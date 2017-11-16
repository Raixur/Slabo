using System.Collections;
using UnityEngine;
using Random = System.Random;

public class LightFlickering : MonoBehaviour
{
    private readonly Random rnd = new Random();

    [SerializeField] private float minTime = 0.1f;
    [SerializeField] private float maxTime = 0.5f;

    [SerializeField] private Light flickeringLight = null;
    
    private float deltaTime;

    public float NextDuration {
        get { return (float) rnd.NextDouble() * deltaTime + minTime; }
    }

    public void Start()
    {
        deltaTime = maxTime - minTime;
    }

    public void FlickerByCount(int count)
    {
        StartCoroutine(FlickerByCountCoroutine(count));
    }

    public void FlickerByTime(float time)
    {
        StartCoroutine(FlickerByTimeCoroutine(time));
    } 

    private IEnumerator FlickerByCountCoroutine(int count)
    {
        for (var i = 0; i < count * 2; i++)
        {
            flickeringLight.enabled = !flickeringLight.enabled;
            yield return new WaitForSeconds(NextDuration);
        }
    }

    private IEnumerator FlickerByTimeCoroutine(float time)
    {
        var lastPhaseDuration = Mathf.Min(NextDuration, time);
        time -= lastPhaseDuration;

        while (time > 0f)
        {
            flickeringLight.enabled = !flickeringLight.enabled;
            var phaseDuration = Mathf.Min(NextDuration, time);
            yield return new WaitForSeconds(phaseDuration);
            time -= lastPhaseDuration;
        }

        flickeringLight.enabled = false;
        yield return new WaitForSeconds(lastPhaseDuration);
        flickeringLight.enabled = true;
    }
}
