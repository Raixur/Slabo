using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FinalRoom : MonoBehaviour
{
    
    [SerializeField] private List<LightSwitch> lights;
    [SerializeField] private float delay = 2f;

    [SerializeField] private Animator shootingAnimator;
    [SerializeField] private AudioSource switchAudio;
    [SerializeField] private AudioSource shotAudio;
    [SerializeField] private float delayBeforeShoot = 1f;


    public void Awake()
    {
        lights.ForEach(l => l.enabled = false);
    }

    public void Start()
    {
        StartCoroutine(FinalSceneCoroutine());
    }

    private IEnumerator FinalSceneCoroutine()
    {
        yield return new WaitForSeconds(delay);
        
        foreach(var l in lights)
        {
            l.SwitchLight();
            yield return new WaitForSeconds(l.Duration);
        }

        yield return new WaitForSeconds(delayBeforeShoot);
        shootingAnimator.SetTrigger("Shoot");
        yield return new WaitForSeconds(0.2f);
        shotAudio.Play();
    }
}
