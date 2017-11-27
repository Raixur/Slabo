using System.Collections;
using AudioSDK;
using JetBrains.Annotations;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private string musicPlayListName = "";

    [UsedImplicitly]
    private void Start ()
	{
        if(!string.IsNullOrEmpty(musicPlayListName))
	        StartCoroutine(StartPlayList());
	}

    private IEnumerator StartPlayList()
    {
        while (FindObjectOfType<AudioListener>() == null)
        {
            yield return null;
        }
        AudioController.PlayMusicPlaylist(musicPlayListName);
    }
}
