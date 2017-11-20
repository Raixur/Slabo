using System.Collections;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioPlaylist : MonoBehaviour
{
    [SerializeField] private AudioClip[] list;

    private System.Random rnd = new System.Random();
    private AudioSource audioSource;

    [UsedImplicitly]
    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(PlaySoundCoroutine());
    }

    private IEnumerator PlaySoundCoroutine()
    {
        while (true)
        {
            var index = rnd.Next(list.Length);
            var clip = list[index];

            audioSource.clip = clip;
            audioSource.Play();

            yield return new WaitForSeconds(clip.length);
        }
    }
}
