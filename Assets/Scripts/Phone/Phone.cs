using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRTK;
using TMPro;
using Random = System.Random;

public class Phone : MonoBehaviour
{
    private const int RequiredDigits = 4;

    [SerializeField] private AudioPlaylist ambient = null;
    [SerializeField] private AudioSource source = null;
    [SerializeField] private List<AudioClip> audioNumbers = null;
    [SerializeField] private float delay = 1.5f;

    [SerializeField] private TextMeshPro display = null;
    [SerializeField] private List<VRTK_Button> buttons = null;
    [SerializeField] private VRTK_Button resetButton = null;
    [SerializeField] private VRTK_Button callButton = null;
    [SerializeField] private string phoneNumber = "0000";

    private int[] code = new int[4];

    private List<int> digits = new List<int>(RequiredDigits);

    public void Start()
    {
        GeneratePhoneCode();
        display.SetText("");
        for (var i = 0; i < buttons.Count; i++)
        {
            var number = i;
            buttons[i].Pushed += (sender, args) => InputNumber(number);
        }

        if(resetButton != null) resetButton.Pushed += (sender, args) => ResetNumbers();
        if(callButton != null) callButton.Pushed += (sender, args) => StartCall();
    }

    public void InputNumber(int number)
    {
        Debug.Log("pressed");
        if (digits.Count < RequiredDigits)
        {
            digits.Add(number);
            UpdateDisplay();
        }
    }

    public void ResetNumbers()
    {
        digits.Clear();
        display.SetText("");
    }

    public void StartCall()
    {
        if (display.text == phoneNumber)
        {
            StartCoroutine(NumberPlayCoroutine());
        }
    }

    private IEnumerator NumberPlayCoroutine()
    {
        ambient.DecreaseVolume(10f, 0.7f);
        var waitDelay = new WaitForSeconds(delay);
        foreach (var number in code)
        {
            source.clip = audioNumbers[number];
            source.Play();
            yield return waitDelay;
        }

        source.Stop();
    }

    public void UpdateDisplay()
    {
        display.SetText(string.Join("", digits.Select(d => d.ToString()).ToArray()));
    }

    private void GeneratePhoneCode()
    {
        var rnd = new Random();
        for (var i = 0; i < code.Length; i++)
        {
            code[i] = rnd.Next(0, 9);
        }
    }
}
