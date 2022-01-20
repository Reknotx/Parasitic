using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance;
    public Animator transition;

    private bool loading = false;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    public void LoadLevel(string level)
    {
        Debug.Log("Loading level: " + level);
        StartCoroutine(LoadLevelCR(level));
    }

    IEnumerator LoadLevelCR(string level)
    {
        if(Upgrades.Instance != null)
        {
            Upgrades.Instance.SaveUpgrades();
        }
        
        //Play the animation
        transition.SetTrigger("FadeOut");

        //Debug.Log("Loading Level");

        //Wait
        yield return new WaitForSeconds(1);

        // AsyncOperation operation = SceneManager.LoadSceneAsync(level);

        PlayerPrefs.SetString("SelectedLevel", level);
        SceneManager.LoadScene("LoadingScreen");

        // while (!operation.isDone)
        // {
        //     yield return null;
        // }
    }

}
