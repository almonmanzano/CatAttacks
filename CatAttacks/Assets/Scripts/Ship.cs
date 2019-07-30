using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
    private float m_velocityEasy = 10f, m_velocityNormal = 15f, m_velocityHard = 20f;
    private float m_distance = 1f;

    private Planet m_target;
    private Planet.PlanetType m_type;
    private int m_count;
    private GameController m_controller;

    private float m_velocity;

    public void Initialize(Planet target, Planet.PlanetType type, int count, GameController controller)
    {
        m_target = target;
        m_type = type;
        m_count = count;
        m_controller = controller;

        GameSettings.GameMode gameMode = m_controller.GetGameMode();
        switch (gameMode)
        {
            case GameSettings.GameMode.Easy:
                m_velocity = m_velocityEasy;
                break;
            case GameSettings.GameMode.Normal:
                m_velocity = m_velocityNormal;
                break;
            case GameSettings.GameMode.Hard:
                if (type == Planet.PlanetType.Enemy)
                {
                    m_velocity = m_velocityHard;
                }
                else
                {
                    m_velocity = m_velocityNormal;
                }
                break;
        }
    }

    private void Update()
    {
        if (m_controller.GameEnd())
        {
            return;
        }

        Vector2 targetPosition = m_target.transform.position;
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, m_velocity * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPosition) < m_distance)
        {
            m_target.GetShip(m_type, m_count);
            Destroy(gameObject);
        }
    }
}
