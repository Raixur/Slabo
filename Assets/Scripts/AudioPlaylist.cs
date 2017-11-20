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
        var index = rnd.Next(list.Length);
        while (true)
        {
            var clip = list[index];

            audioSource.clip = clip;
            audioSource.Play();

            yield return new WaitForSeconds(clip.length);

            if (list.Length > 1)
            {
                var previousIndex = index;
                do
                {
                    index = rnd.Next(list.Length);
                } while (previousIndex == index);
            }
        }
    }

    public void DecreaseVolume(float time, float volume)
    {
        StartCoroutine(DecreaseVolumeCoroutine(time, volume));
    }

    private IEnumerator DecreaseVolumeCoroutine(float time, float volume)
    {
        audioSource.volume = audioSource.volume - volume / 2;
        yield return new WaitForSeconds(0.1f);
        audioSource.volume = audioSource.volume - volume / 2;

        yield return new WaitForSeconds(time - 0.2f);
        audioSource.volume = audioSource.volume + volume / 2;
        yield return new WaitForSeconds(0.1f);
        audioSource.volume = audioSource.volume + volume / 2;
    }
}
