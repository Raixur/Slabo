using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRTK;

[RequireComponent(typeof(VRTK_HeadsetFade))]
public class FinalRoom : MonoBehaviour
{
    [SerializeField] private List<LightSwitch> lights;
    [SerializeField] private float delay = 2f;

    [SerializeField] private Animator shootingAnimator;
    [SerializeField] private AudioSource switchAudio;
    [SerializeField] private AudioSource shotAudio;
    [SerializeField] private float delayBeforeShoot = 1f;
    [SerializeField] private float delayBeforeFade = 1f;
    [SerializeField] private float transitionDuration = 4f;
    [SerializeField] private string menuScene;

    private VRTK_HeadsetFade fade;

    public void Awake()
    {
        fade = GetComponent<VRTK_HeadsetFade>();
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
            yield return new WaitForSeconds(l.Duration);
            l.SwitchLight();
        }

        yield return new WaitForSeconds(delayBeforeShoot);
        shootingAnimator.SetTrigger("Shoot");
        yield return new WaitForSeconds(0.45f);
        shotAudio.Play();

        yield return new WaitForSeconds(delayBeforeFade);
        fade.Fade(Color.black, transitionDuration);
        yield return new WaitForSeconds(transitionDuration);

        var loading = SceneManager.LoadSceneAsync(menuScene);
        while (!loading.isDone)
        {
            yield return null;
        }

        fade.Unfade(transitionDuration);
    }
}
