using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(Light))]
[RequireComponent(typeof(AudioSource))]
public class LightSwitch : MonoBehaviour
{
    private new Light light;
    private new AudioSource audio;

    [SerializeField] private float audioDelay = 0f;

    public float Duration = 1f;
    
	// Use this for initialization
    [UsedImplicitly]
    private void Awake()
    {
        light = GetComponent<Light>();
        audio = GetComponent<AudioSource>();
    }

    public void SwitchLight() { StartCoroutine(SwitchLightCoroutine()); }

    private IEnumerator SwitchLightCoroutine()
    {
        audio.Play();
        yield return new WaitForSeconds(audioDelay);
        light.enabled = !light.enabled;
    }
}
