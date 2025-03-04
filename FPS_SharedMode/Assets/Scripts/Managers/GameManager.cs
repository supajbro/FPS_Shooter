using Fusion;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;

    [Header("Player")]
    private PlayerMovement m_localPlayer;
    public void SetLocalPlayer(PlayerMovement player) { m_localPlayer = player; }
    public PlayerMovement GetLocalPlayer() { return m_localPlayer; }
    public List<Movement> GetAllPlayers()
    {
        return FindObjectsOfType<Movement>().ToList();
    }

    [Header("Bots")]
    [SerializeField] private GameObject m_botPoolerPrefab;

    [Header("UI")]
    public PlayerInfo PlayerInfo;
    [SerializeField] private CanvasGroup m_gameOverScreen;
    private float m_gameOverTimer = 0.0f;
    private const float MaxGameOverTime = 5.0f;

    [Header("Spawn Points")]
    public List<Transform> spawnPoints;

    private void Awake()
    {
        instance = this;
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        GameOverScreenUpdate();
    }

    public void SpawnBotPooler(NetworkRunner Runner)
    {
        Runner.Spawn(m_botPoolerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
    }

    private void GameOverScreenUpdate()
    {
        if(GetLocalPlayer() == null)
        {
            return;
        }

        if (GetLocalPlayer().GameOver)
        {
            m_gameOverScreen.alpha += Time.deltaTime;
            m_gameOverScreen.blocksRaycasts = true;
            m_gameOverScreen.interactable = true;

            m_gameOverTimer += Time.deltaTime;

            if(m_gameOverTimer >= MaxGameOverTime)
            {
                GetLocalPlayer().GameOver = false;
            }
        }
        else
        {
            m_gameOverScreen.alpha -= Time.deltaTime;
            m_gameOverScreen.blocksRaycasts = false;
            m_gameOverScreen.interactable = false;

            m_gameOverTimer = 0.0f;
        }
    }

}
