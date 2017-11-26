using System.Collections;
using AudioSDK;
using JetBrains.Annotations;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private string ambiencePlayListName = "";

    [UsedImplicitly]
    private void Start ()
	{
        if(string.IsNullOrEmpty(ambiencePlayListName))
	    StartCoroutine(StartPlayList());
	}

    private IEnumerator StartPlayList()
    {
        while (FindObjectOfType<AudioListener>() == null)
        {
            yield return null;
        }
        AudioController.PlayMusicPlaylist(ambiencePlayListName);
    }
}
