using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private string playListName = null;

	void Start ()
	{
	    StartCoroutine(StartPlayList());
	}

    private IEnumerator StartPlayList()
    {
        while (FindObjectOfType<AudioListener>() == null)
        {
            yield return null;
        }
        AudioController.PlayMusicPlaylist(playListName);
    }
}
