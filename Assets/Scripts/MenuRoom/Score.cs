using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(LevelSwitcher))]
public class Score : MonoBehaviour
{
    public string FileName = "lb.bin";
    private DateTime startTime;

    // Use this for initialization
    [UsedImplicitly]
    private void Start ()
    {
        startTime = DateTime.Now;
        GetComponent<LevelSwitcher>().Finish += (sender, args) => FinishGame();
    }

    public void FinishGame()
    {
        var score = DateTime.Now - startTime;
        SaveScore(score);
    }

    private void SaveScore(TimeSpan score)
    {
        var lastScore = GetLastScore();
        if (score < lastScore)
        {
            var bf = new BinaryFormatter();
            using (var file = File.Create(Path.Combine(Application.persistentDataPath, FileName)))
            {

                bf.Serialize(file, score);
            }
        }
    }

    private TimeSpan GetLastScore()
    {
        var savePath = Path.Combine(Application.persistentDataPath, FileName);
        if (File.Exists(savePath))
        {
            var bf = new BinaryFormatter();
            using (var file = File.OpenRead(savePath))
            {
                return (TimeSpan)bf.Deserialize(file);
            }
        }

        return TimeSpan.MaxValue;
    }
}
