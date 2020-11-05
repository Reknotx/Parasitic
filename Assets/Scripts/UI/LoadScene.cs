using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public Image progress;

    void Start()
    {
        StartCoroutine(LoadNextScene());
    }

    IEnumerator LoadNextScene()
    {
        yield return new WaitForSeconds(0.1f);
        string NextScene = PlayerPrefs.GetString("SelectedLevel");
        if (Application.CanStreamedLevelBeLoaded(NextScene))
        {
            AsyncOperation gameLevel = SceneManager.LoadSceneAsync(NextScene);
            while (gameLevel.progress < 1)
            {
                progress.fillAmount = gameLevel.progress;
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            Debug.LogError("Invalid Scene Name: " + NextScene);
            SceneManager.LoadScene(0);
        }
    }
}
