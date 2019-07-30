using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Planet : MonoBehaviour
{
    public Text m_num;

    private float m_waitingTime = 1f;
    private float m_increaseRateEasy = 0.75f, m_increaseRateNormal = 0.5f, m_increaseRateHard = 0.25f;

    private float m_range = 5f;

    private float m_scale1 = 1f, m_scale5 = 1.5f, m_scale10 = 2f, m_scale50 = 2.5f;

    public GameObject m_selection;

    public GameObject m_friendlyShip, m_enemyShip;
    public Sprite m_spriteNeutral, m_spriteFriendly, m_spriteEnemy;

    public enum PlanetType { Null, Neutral, Friendly, Enemy };

    private GameController m_controller;
    private PlanetType m_type = PlanetType.Null;
    private GameObject m_ship;
    private int m_numShips = 0;
    private bool m_selected = false;
    private GameObject m_selectionInstance = null;

    private float m_increaseRate;

    public void Initialize(GameController controller, PlanetType type)
    {
        m_controller = controller;
        GameSettings.GameMode gameMode = m_controller.GetGameMode();
        switch (gameMode)
        {
            case GameSettings.GameMode.Easy:
                m_increaseRate = m_increaseRateEasy;
                break;
            case GameSettings.GameMode.Normal:
                m_increaseRate = m_increaseRateNormal;
                break;
            case GameSettings.GameMode.Hard:
                if (type == Planet.PlanetType.Enemy)
                {
                    m_increaseRate = m_increaseRateHard;
                }
                else
                {
                    m_increaseRate = m_increaseRateNormal;
                }
                break;
        }

        SetType(type);
    }

    private void SetType(PlanetType type)
    {
        PlanetType oldType = m_type;
        m_type = type;

        if (m_type == PlanetType.Enemy)
        {
            m_controller.AddEnemyPlanet(this);

            if (oldType != PlanetType.Null)
            {
                m_controller.RemoveNotEnemyPlanet(this);

                if (oldType == PlanetType.Friendly)
                {
                    m_controller.RemoveFriendlyPlanet(this);
                }
            }

            gameObject.GetComponent<SpriteRenderer>().sprite = m_spriteEnemy;
            m_ship = m_enemyShip;
            m_num.color = Color.yellow;
        }
        else if (m_type == PlanetType.Friendly)
        {
            m_controller.AddFriendlyPlanet(this);

            if (oldType != PlanetType.Neutral)
            {
                m_controller.AddNotEnemyPlanet(this);

                if (oldType == PlanetType.Enemy)
                {
                    m_controller.RemoveEnemyPlanet(this);
                }
            }

            gameObject.GetComponent<SpriteRenderer>().sprite = m_spriteFriendly;
            m_ship = m_friendlyShip;
            m_num.color = Color.blue;
        }
        else
        {
            m_controller.AddNotEnemyPlanet(this);

            gameObject.GetComponent<SpriteRenderer>().sprite = m_spriteNeutral;
            m_num.text = "";
        }

        CancelInvoke();

        if (m_type != PlanetType.Neutral)
        {
            if (m_ship == null)
            {
                Debug.LogError("Ship prefab is null.");
                return;
            }
            InvokeRepeating("IncreaseNumShips", m_waitingTime, m_increaseRate);
        }

        // If the enemies have invaded a selected friendly planet
        if (m_selectionInstance != null)
        {
            Destroy(m_selectionInstance);
            m_controller.DeselectPlanet(this);
        }
    }

    private void IncreaseNumShips()
    {
        if (m_controller.GameEnd())
        {
            CancelInvoke();
            return;
        }

        SetNumShips(m_numShips + 1);
    }

    private void SetNumShips(int n)
    {
        m_numShips = n;
        m_num.text = m_numShips.ToString();
    }

    private void OnMouseDown()
    {
        if (m_controller != null && m_controller.GameEnd())
        {
            return;
        }

        if (m_type == PlanetType.Friendly)
        {
            if (!m_selected)
            {
                Select();
                m_controller.SelectPlanet(this);
            }
            else
            {
                Deselect();
                m_controller.DeselectPlanet(this);
            }
        }
        else
        {
            m_controller.SendShips(this);
        }
    }

    public void Select()
    {
        m_selected = true;
        if (m_type == PlanetType.Friendly)
        {
            m_selectionInstance = Instantiate(m_selection, transform.position, Quaternion.identity, transform);
        }
    }

    private void Deselect()
    {
        m_selected = false;
        if (m_type == PlanetType.Friendly)
        {
            Destroy(m_selectionInstance);
        }
    }

    public int GetNumShips()
    {
        return m_numShips;
    }

    public void SendShips(Planet planet)
    {
        Deselect();

        int nShips = m_numShips;

        while (nShips >= 50)
        {
            Vector2 rand = new Vector2(Random.Range(-m_range, m_range), Random.Range(-m_range, m_range));
            GameObject shipGameObject = Instantiate(m_ship, (Vector2)transform.position + rand, Quaternion.identity);
            shipGameObject.transform.localScale *= m_scale50;
            Ship ship = shipGameObject.GetComponent<Ship>();
            ship.Initialize(planet, m_type, 50, m_controller);

            nShips -= 50;
        }

        while (nShips >= 10)
        {
            Vector2 rand = new Vector2(Random.Range(-m_range, m_range), Random.Range(-m_range, m_range));
            GameObject shipGameObject = Instantiate(m_ship, (Vector2)transform.position + rand, Quaternion.identity);
            shipGameObject.transform.localScale *= m_scale10;
            Ship ship = shipGameObject.GetComponent<Ship>();
            ship.Initialize(planet, m_type, 10, m_controller);

            nShips -= 10;
        }

        if (nShips >= 5)
        {
            Vector2 rand = new Vector2(Random.Range(-m_range, m_range), Random.Range(-m_range, m_range));
            GameObject shipGameObject = Instantiate(m_ship, (Vector2)transform.position + rand, Quaternion.identity);
            shipGameObject.transform.localScale *= m_scale5;
            Ship ship = shipGameObject.GetComponent<Ship>();
            ship.Initialize(planet, m_type, 5, m_controller);

            nShips -= 5;
        }

        for (int i = 0; i < nShips; i++)
        {
            Vector2 rand = new Vector2(Random.Range(-m_range, m_range), Random.Range(-m_range, m_range));
            GameObject shipGameObject = Instantiate(m_ship, (Vector2)transform.position + rand, Quaternion.identity);
            shipGameObject.transform.localScale *= m_scale1;
            Ship ship = shipGameObject.GetComponent<Ship>();
            ship.Initialize(planet, m_type, 1, m_controller);
        }

        SetNumShips(0);
    }

    public void GetShip(PlanetType type, int n)
    {
        if (m_type != type)
        {
            if (m_numShips - n <= 0)
            {
                SetType(type);
                SetNumShips(Mathf.Abs(m_numShips - n));
            }
            else
            {
                SetNumShips(m_numShips - n);
            }
        }
        else
        {
            SetNumShips(m_numShips + n);
        }
    }

    public bool IsSelected()
    {
        return m_selected;
    }
}
