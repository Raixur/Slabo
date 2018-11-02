using System.Collections;
using AudioSDK;
using JetBrains.Annotations;
using UnityEngine;
using Random = System.Random;

public class Taser : MonoBehaviour
{
    [SerializeField] private GameObject lightningFx;
    [SerializeField] private float duration = 1f;

    private string taserAudio = "Taser";
    private Light pulstaingLight;
    private float dt = 0.1f;

    private float minIntensity = 0.9f;
    private float maxIntenstity = 1.5f;

    private float minRange = 0.9f;
    private float maxRange = 1.2f;

    [UsedImplicitly]
    private void Awake()
    {
        pulstaingLight = GetComponentInChildren<Light>();
        pulstaingLight.enabled = false;
        lightningFx.SetActive(false);
    }

    public void Activate()
    {
        StartCoroutine(PulationCoroutine());
    }

    private IEnumerator PulationCoroutine()
    {
        lightningFx.SetActive(true);
        pulstaingLight.enabled = true;
        AudioController.Play(taserAudio);

        var rnd = new Random();
        var dIntensity = maxIntenstity - minIntensity;
        var dRange = maxRange - minRange;
        for (var i = 0f; i < duration; i+= dt)
        {
            pulstaingLight.intensity = (float)rnd.NextDouble() * dIntensity + minIntensity;
            pulstaingLight.range = (float) rnd.NextDouble() * dRange + minRange;
            yield return new WaitForSeconds(dt);
        }

        lightningFx.SetActive(false);
        pulstaingLight.enabled = false;
    }
}