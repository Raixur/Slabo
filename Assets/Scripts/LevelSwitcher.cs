using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRTK;

[RequireComponent(typeof(VRTK_Control))]
[RequireComponent(typeof(VRTK_HeadsetFade))]
public class LevelSwitcher : MonoBehaviour
{
    [SerializeField] private string scene;
    [SerializeField] private float value = 60;
    [SerializeField] private float fadeTransiotion = 0.5f;
    [SerializeField] private Color fadeColor = Color.black;

    private VRTK_HeadsetFade fade;

    public void Start()
    {
        fade = GetComponent<VRTK_HeadsetFade>();
        GetComponent<VRTK_Control>().ValueChanged += SwitchScene;
    }

    private void SwitchScene(object sender, Control3DEventArgs args)
    {
        if (args.normalizedValue >= value)
        {
            StartCoroutine(LoadSceneCoroutine());
        }
            
    }

    private IEnumerator LoadSceneCoroutine()
    {
        OnFinish();
        // The Application loads the Scene in the background at the same time as the current Scene.
        //This is particularly good for creating loading screens. You could also load the Scene by build //number.

        fade.Fade(fadeColor, fadeTransiotion);
        yield return new WaitForSeconds(fadeTransiotion);

        var asyncLoad = SceneManager.LoadSceneAsync(scene); 

        //Wait until the last operation fully loads to return anything
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        fade.Unfade(fadeTransiotion);
    }

    public event EventHandler Finish;

    protected virtual void OnFinish()
    {
        if (Finish != null)
            Finish(this, EventArgs.Empty);
    }
}
