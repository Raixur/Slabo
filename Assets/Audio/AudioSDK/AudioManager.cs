using AudioSDK;
using JetBrains.Annotations;
using UnityEngine;
using VRTK;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private string musicPlayListName = "";

    [UsedImplicitly]
    private void Start ()
	{
        if(!string.IsNullOrEmpty(musicPlayListName))
	        VRTK_SDKManager.instance.LoadedSetupChanged += (sender, args) => AudioController.PlayMusicPlaylist(musicPlayListName);
    }
}
