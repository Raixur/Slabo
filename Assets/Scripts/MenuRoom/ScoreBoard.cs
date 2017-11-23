using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnityEngine;

public class ScoreBoard : MonoBehaviour
{
    public string fileName = "lb.bin";
    public string defaultScore = "NO ESCAPE\n";
    public TextMeshPro textDisplay;


	// Use this for initialization
	void Start ()
    {
        LoadScore();
    }

    private void LoadScore()
    {
        var score = defaultScore;
        var savePath = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(savePath))
        {
            var bf = new BinaryFormatter();
            using (var file = File.OpenRead(savePath))
            {
                var time = (TimeSpan) bf.Deserialize(file);
                score += string.Format("{0:D2}:{1:D2}:{2:D2}", time.Hours, time.Minutes, time.Seconds);
            }
        }

        textDisplay.SetText(score);    
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
