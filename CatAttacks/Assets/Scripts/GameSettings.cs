using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public enum GameMode { Easy, Normal, Hard };

    private GameMode m_gameMode;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void PlayEasy()
    {
        m_gameMode = GameMode.Easy;
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    public void PlayNormal()
    {
        m_gameMode = GameMode.Normal;
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    public void PlayHard()
    {
        m_gameMode = GameMode.Hard;
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    public GameMode GetGameMode()
    {
        return m_gameMode;
    }
}
