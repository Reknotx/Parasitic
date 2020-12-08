using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;

    private bool loading = false;

    // Update is called once per frame
    //void Update()
    //{
    //    if (CombatSystem.Instance == null) return;

    //    if (CombatSystem.Instance.state == BattleState.Won && loading == false)
    //    {
    //        Debug.Log("Level Won");
    //        LoadNextLevel();
    //    }
    //}

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

        AsyncOperation operation = SceneManager.LoadSceneAsync(level);

        while (!operation.isDone)
        {
            yield return null;
        }
    }

}
