using DG.Tweening;
using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public PlayerRef player;

    [Header("Bots")]
    [SerializeField] private GameObject m_botPoolerPrefab;

    [Header("UI")]
    public PlayerInfo PlayerInfo;
    public MainMenu MainMenu;
    [SerializeField] private CanvasGroup m_playerScreen;
    [SerializeField] private CanvasGroup m_waitingScreen;
    [SerializeField] private CanvasGroup m_gameOverScreen;
    [SerializeField] private TextMeshProUGUI m_gameOverText;
    private string m_winString = "";
    public LoadingVisual LoadingVisual;
    private float m_gameOverTimer = 0.0f;
    private const float MaxGameOverTime = 5.0f;
    [SerializeField] private Image m_transition;
    [SerializeField] private Toggle m_fullScreenToggle;
    public Slider sensitivity;

    [Header("Waiting UI")]
    [SerializeField] private TextMeshProUGUI m_playerAliveText;
    [SerializeField] private TextMeshProUGUI m_playerDeadText;

    [Header("Spawn Points")]
    public List<Transform> spawnPoints;

    private void Awake()
    {
        instance = this;
        Application.targetFrameRate = 60;

        // Ensure the toggle reflects the current fullscreen state
        m_fullScreenToggle.isOn = Screen.fullScreen;

        // Add a listener to handle toggle changes
        m_fullScreenToggle.onValueChanged.AddListener(SetFullscreen);
    }

    private void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    private void Update()
    {
        GameOverScreenUpdate();

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            OpenShutdownPopup();
        }
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

        if (GetLocalPlayer().IsDead)
        {
            m_waitingScreen.alpha += Time.deltaTime * FadeSpeed;
            m_waitingScreen.blocksRaycasts = true;
            m_waitingScreen.interactable = true;
            UpdateWaitingUI();

            m_gameOverScreen.alpha -= Time.deltaTime * FadeSpeed;
            m_gameOverScreen.blocksRaycasts = false;
            m_gameOverScreen.interactable = false;
            m_winString = "";

            m_gameOverTimer = 0.0f;
        }
        else if (GetLocalPlayer().GameOver)
        {
            m_gameOverScreen.alpha += Time.deltaTime * FadeSpeed;
            m_gameOverScreen.blocksRaycasts = true;
            m_gameOverScreen.interactable = true;

            m_waitingScreen.alpha -= Time.deltaTime * FadeSpeed;
            m_waitingScreen.blocksRaycasts = false;
            m_waitingScreen.interactable = false;

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

            m_waitingScreen.alpha -= Time.deltaTime * FadeSpeed;
            m_waitingScreen.blocksRaycasts = false;
            m_waitingScreen.interactable = false;

            m_gameOverTimer = 0.0f;
        }
    }

    private void UpdateWaitingUI()
    {
        List<string> aliveNames = new();
        List<string> deadNames = new();

        foreach (var player in GetAllPlayers())
        {
            if (player.IsDead)
            {
                deadNames.Add(player.PlayerName);
            }
            else
            {
                aliveNames.Add(player.PlayerName);
            }
        }

        m_playerAliveText.text = string.Join("\n", aliveNames);
        m_playerDeadText.text = string.Join("\n", deadNames);
    }

    public void OpenShutdownPopup()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Popup.Instance.Display(false, ShutddownGame, Popup.Instance.Close, "Do you want to exit the game?", "Exit Game", "Close");
    }

    public void ShutddownGame()
    {
        Popup.Instance.Close();
        GetLocalPlayer().Shutdown();
        MainMenu.InitMainMenu();
        m_playerScreen.alpha = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void PlayTransition()
    {
        return;
        const float Value = 0.5f;
        m_transition.DOFillAmount(1.0f, Value).OnComplete(() =>
        {
            DOVirtual.DelayedCall(Value, () => m_transition.DOFillAmount(0.0f, Value), true);
        });

    }
}
