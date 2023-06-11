using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Y3ADV;

public class PlayControl : MonoBehaviour
{
    public void Play()
    {
        GameManager.Play();
        UIManager.Instance.PlayCanvas.SetActive(false);
    }

    public void Replay()
    {
        SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
    }
}
