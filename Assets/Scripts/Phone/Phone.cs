using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRTK;

public class Phone : MonoBehaviour
{
    [SerializeField] private DigitInputPanel panel = null;
    [SerializeField] private AudioSource source = null;
    [SerializeField] private List<AudioClip> audioNumbers = null;
    [SerializeField] private float delay = 1.5f;
    
    [SerializeField] private VRTK_Button callButton = null;

    private string phoneNumber;
    private int[] doorCode;

    public void Start()
    {
        var codeGen = CodeGenerator.Instance;
        phoneNumber = codeGen.PhoneNumber.Aggregate("", (c, d) => c + d);
        doorCode = codeGen.DoorCode;
;
        if(callButton != null) callButton.Pushed += (sender, args) => StartCall();
    }

    public void StartCall()
    {
        if (panel.Value == phoneNumber)
            StartCoroutine(NumberPlayCoroutine());
    }

    // Rework audio interaction
    private IEnumerator NumberPlayCoroutine()
    {
        // TODO: decrease music volume

        var waitDelay = new WaitForSeconds(delay);
        foreach (var number in doorCode)
        {
            source.clip = audioNumbers[number];
            source.Play();
            yield return waitDelay;
        }

        source.Stop();
    }
}
