using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AudioSDK;
using UnityEngine;
using VRTK;

public class Phone : MonoBehaviour
{
    public const string NumbersPlaylistName = "Numbers";

    [SerializeField] private DigitInputPanel panel = null;
    [SerializeField] private VRTK_Button callButton = null;

    [SerializeField] private AudioObject phoneAudio = null;
    [SerializeField] private AudioObject buttonAudio = null;
    [SerializeField] private string buttonClickAudio = "";
    [SerializeField] private float delay = 1.2f;

    private string phoneNumber;
    private int[] doorCode;

    public void Start()
    {
        var codeGen = CodeGenerator.Instance;
        phoneNumber = codeGen.PhoneNumber.Aggregate("", (c, d) => c + d);
        Debug.Log("Phone: " + phoneNumber);

        //var numbersPlaylist = codeGen.PhoneNumber.Select(d => d.ToString()).ToArray();
        //AudioController.AddPlaylist(NumbersPlaylistName, numbersPlaylist);

        doorCode = codeGen.DoorCode;
;
        if(callButton != null) callButton.Pushed += (sender, args) => StartCall();
    }

    public void StartCall()
    {
        Debug.Log("Play");
        buttonAudio.PlayAfter(buttonClickAudio);
        if (panel.Value == phoneNumber)
            StartCoroutine(PlayNumberCoroutine());
    }

    public IEnumerator PlayNumberCoroutine()
    {
        var delayWait = new WaitForSeconds(delay);
        foreach (var number in doorCode)
        {
            phoneAudio.PlayAfter(number.ToString());
            yield return delayWait;
        }
    }
}
