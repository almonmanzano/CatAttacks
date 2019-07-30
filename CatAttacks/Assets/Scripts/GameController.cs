using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject m_planet;
    public GameObject m_friendlyPlanet, m_enemyPlanet;
    public GameObject[] m_neutralPlanets;
    public GameObject m_background;
    public Sprite m_bgEasy, m_bgNormal, m_bgHard;
    public GameObject m_youWin, m_gameOver;
    public AudioSource m_audioWin, m_audioLose;

    private float m_AIWaitingTimeEasy = 3f, m_AIThinkingTimeEasy = 2f;
    private float m_AIWaitingTimeNormal = 2f, m_AIThinkingTimeNormal = 1f;
    private float m_AIWaitingTimeHard = 2f, m_AIThinkingTimeHard = 0.5f;
    private float m_AIProbAttack = 0.5f;

    private int m_numPlanets;
    private Planet[] m_planets;
    private List<Planet> m_selectedPlanets = new List<Planet>();

    private List<Planet> m_enemyPlanets = new List<Planet>();
    private List<Planet> m_notEnemyPlanets = new List<Planet>();
    private List<Planet> m_friendlyPlanets = new List<Planet>();

    private bool m_endGame = false;
    private bool m_canGoToMenu = false;

    private float m_AIWaitingTime, m_AIThinkingTime;

    private GameSettings.GameMode m_gameMode;

    private void Start()
    {
        SetSettings();

        AddPlanets();

        InvokeRepeating("AIThink", m_AIWaitingTime, m_AIThinkingTime);
    }

    private void SetSettings()
    {
        GameSettings settings = FindObjectOfType<GameSettings>();
        if (settings == null)
        {
            Debug.LogError("Settings not found.");
        }

        m_gameMode = settings.GetGameMode();

        switch (m_gameMode)
        {
            case GameSettings.GameMode.Easy:
                m_AIWaitingTime = m_AIWaitingTimeEasy;
                m_AIThinkingTime = m_AIThinkingTimeEasy;

                m_background.GetComponent<SpriteRenderer>().sprite = m_bgEasy;
                break;
            case GameSettings.GameMode.Normal:
                m_AIWaitingTime = m_AIWaitingTimeNormal;
                m_AIThinkingTime = m_AIThinkingTimeNormal;

                m_background.GetComponent<SpriteRenderer>().sprite = m_bgNormal;
                break;
            case GameSettings.GameMode.Hard:
                m_AIWaitingTime = m_AIWaitingTimeHard;
                m_AIThinkingTime = m_AIThinkingTimeHard;

                m_background.GetComponent<SpriteRenderer>().sprite = m_bgHard;
                break;
        }

        Destroy(settings.gameObject);
    }

    private void AddPlanets()
    {
        if (m_planet == null || m_friendlyPlanet == null || m_enemyPlanet == null)
        {
            Debug.LogError("Planet prefab is null.");
            return;
        }

        m_numPlanets = m_neutralPlanets.Length + 2;
        m_planets = new Planet[m_numPlanets];

        m_planets[0] = m_friendlyPlanet.GetComponent<Planet>();
        m_planets[0].Initialize(this, Planet.PlanetType.Friendly);

        m_planets[1] = m_enemyPlanet.GetComponent<Planet>();
        m_planets[1].Initialize(this, Planet.PlanetType.Enemy);

        for (int i = 2; i < m_numPlanets; i++)
        {
            m_planets[i] = m_neutralPlanets[i - 2].GetComponent<Planet>();
            m_planets[i].Initialize(this, Planet.PlanetType.Neutral);
        }
    }

    public void SelectPlanet(Planet planet)
    {
        m_selectedPlanets.Add(planet);
    }

    public void DeselectPlanet(Planet planet)
    {
        m_selectedPlanets.Remove(planet);
    }

    public void SendShips(Planet planet)
    {
        foreach (Planet selectedPlanet in m_selectedPlanets)
        {
            selectedPlanet.SendShips(planet);
        }
        m_selectedPlanets.Clear();
    }

    public void AddEnemyPlanet(Planet planet)
    {
        m_enemyPlanets.Add(planet);
    }

    public void RemoveEnemyPlanet(Planet planet)
    {
        m_enemyPlanets.Remove(planet);

        if (m_enemyPlanets.Count == 0)
        {
            EndGame(true);
        }
    }

    public void AddNotEnemyPlanet(Planet planet)
    {
        m_notEnemyPlanets.Add(planet);
    }

    public void RemoveNotEnemyPlanet(Planet planet)
    {
        m_notEnemyPlanets.Remove(planet);
    }

    public void AddFriendlyPlanet(Planet planet)
    {
        m_friendlyPlanets.Add(planet);
    }

    public void RemoveFriendlyPlanet(Planet planet)
    {
        m_friendlyPlanets.Remove(planet);

        if (m_friendlyPlanets.Count == 0)
        {
            EndGame(false);
        }
    }

    private void AIThink()
    {
        List<Planet> selectedEnemyPlanets = GetSelectedEnemyPlanets();
        bool noSelected = selectedEnemyPlanets.Count > 0;
        bool allSelected = selectedEnemyPlanets.Count == m_enemyPlanets.Count;
        bool attack = noSelected && m_AIProbAttack > Random.value || allSelected;

        if (attack)
        {
            Planet target = m_notEnemyPlanets[Random.Range(0, m_notEnemyPlanets.Count)];
            foreach (Planet planet in selectedEnemyPlanets)
            {
                planet.SendShips(target);
            }
        }
        else
        {
            List<Planet> notSelectedEnemyPlanets = new List<Planet>();
            if (selectedEnemyPlanets.Count > 0)
            {
                notSelectedEnemyPlanets = GetNotSelectedEnemyPlanets();
            }
            else
            {
                notSelectedEnemyPlanets = m_enemyPlanets;
            }
            Planet planet = notSelectedEnemyPlanets[Random.Range(0, notSelectedEnemyPlanets.Count)];
            planet.Select();
        }
    }

    private List<Planet> GetSelectedEnemyPlanets()
    {
        List<Planet> selectedEnemyPlanets = new List<Planet>();

        foreach (Planet planet in m_enemyPlanets)
        {
            if (planet.IsSelected())
            {
                selectedEnemyPlanets.Add(planet);
            }
        }

        return selectedEnemyPlanets;
    }

    private List<Planet> GetNotSelectedEnemyPlanets()
    {
        List<Planet> selectedEnemyPlanets = new List<Planet>();

        foreach (Planet planet in m_enemyPlanets)
        {
            if (!planet.IsSelected())
            {
                selectedEnemyPlanets.Add(planet);
            }
        }

        return selectedEnemyPlanets;
    }

    private void EndGame(bool win)
    {
        m_endGame = true;

        CancelInvoke();

        if (win)
        {
            m_youWin.SetActive(true);
            m_audioWin.Play();
        }
        else
        {
            m_gameOver.SetActive(true);
            m_audioLose.Play();
        }

        Invoke("CanGoToMenu", 3f);
    }

    public bool GameEnd()
    {
        return m_endGame;
    }

    private void CanGoToMenu()
    {
        m_canGoToMenu = true;
    }

    private void Update()
    {
        if (m_canGoToMenu && Input.GetMouseButtonDown(0))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }
    }

    public GameSettings.GameMode GetGameMode()
    {
        return m_gameMode;
    }
}
