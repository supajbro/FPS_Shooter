using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
    public MainMenu MainMenu;
    [SerializeField] private CanvasGroup m_playerScreen;
    [SerializeField] private CanvasGroup m_gameOverScreen;
    [SerializeField] private TextMeshProUGUI m_gameOverText;
    private string m_winString = "";
    public LoadingVisual LoadingVisual;
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

    public void OpenPlayerScreen()
    {
        m_playerScreen.alpha = 1.0f;
        m_playerScreen.interactable = true;
        m_playerScreen.blocksRaycasts = true;
    }

    public void ClosePlayerScreen()
    {
        m_playerScreen.alpha = 0.0f;
        m_playerScreen.interactable = false;
        m_playerScreen.blocksRaycasts = false;
    }


    private void GameOverScreenUpdate()
    {
        if(GetLocalPlayer() == null)
        {
            return;
        }

        const float FadeSpeed = 2.0f;

        if (GetLocalPlayer().GameOver)
        {
            m_gameOverScreen.alpha += Time.deltaTime * FadeSpeed;
            m_gameOverScreen.blocksRaycasts = true;
            m_gameOverScreen.interactable = true;

            m_gameOverTimer += Time.deltaTime;

            if(m_gameOverTimer >= MaxGameOverTime)
            {
                GetLocalPlayer().GameOver = false;
            }

            if (m_winString == "")
            {
                foreach (var player in GetAllPlayers())
                {
                    if (player.HasWon)
                    {
                        m_winString = $"Game Over!\r\n {player.PlayerName} has won";
                        m_gameOverText.text = m_winString;
                    }
                }
            }
        }
        else
        {
            m_gameOverScreen.alpha -= Time.deltaTime * FadeSpeed;
            m_gameOverScreen.blocksRaycasts = false;
            m_gameOverScreen.interactable = false;
            m_winString = "";

            m_gameOverTimer = 0.0f;
        }
    }
}
