using UnityEngine;
using UnityEngine.SceneManagement;
using Y3ADV;

public class StartSceneManager : MonoBehaviour
{
    protected void Start()
    {
        if (StartupSettings.SpecifiedScenario)
        {
            GameManager.StartScenario(StartupSettings.SpecifiedScenarioName);
            SceneManager.UnloadSceneAsync("StartScene");
        }
    }

    public static void StartGame()
    {
        SceneManager.LoadScene("SelectionScene");
    }
}