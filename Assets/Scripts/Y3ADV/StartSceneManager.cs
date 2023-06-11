using UnityEngine;
using UnityEngine.SceneManagement;
using Y3ADV;

public class StartSceneManager : MonoBehaviour
{
    protected void Start()
    {
        CommandLineArguments.LogParams();

        if (CommandLineArguments.HasArg("scenario"))
        {
            GameManager.StartScenario(CommandLineArguments.GetArgParam("scenario"));
            SceneManager.UnloadSceneAsync("StartScene");
        }
    }

    public static void StartGame()
    {
        SceneManager.LoadScene("SelectionScene");
    }
}