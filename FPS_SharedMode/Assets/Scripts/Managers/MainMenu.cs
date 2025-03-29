using UnityEngine;
using UnityEngine.UI;
using Fusion;
using Fusion.Sockets;
using System.Threading.Tasks;
using TMPro;
using System.Collections.Generic;
using System;
using Fusion.Photon.Realtime;
using System.Text;
using Unity.Mathematics;

public class MainMenu : MonoBehaviour
{

    [Header("Network")]
    [SerializeField] private FusionBootstrap m_fusion;
    [SerializeField] private string m_currentSessionName = "";
    [SerializeField] private string m_playerName = "";

    public FusionBootstrap Fusion => m_fusion;
    public string CurrentSessionName => m_currentSessionName;
    public string PlayerName => m_playerName;

    [SerializeField] private NetworkRunner m_networkRunner;
    [SerializeField] private NetworkRunner runnerPrefab;

    [SerializeField] private Button m_startGameButton;
    [SerializeField] private Button m_joinSessionButton;
    [SerializeField] private CanvasGroup m_mainMenuUI;
    [SerializeField] private TMP_InputField m_playerNameInputField;

    [Header("UI")]
    public CanvasGroup m_canvasGroup;

    public void InitMainMenu()
    {
        OpenMainMenu();
        GameManager.instance.LoadingVisual.gameObject.SetActive(false);
        m_mainMenuUI.alpha = 1.0f;
        m_mainMenuUI.blocksRaycasts = true;
        m_mainMenuUI.interactable = true;
    }

    public void OpenMainMenu()
    {
        m_canvasGroup.alpha = 1.0f;
        m_canvasGroup.interactable = true;
        m_canvasGroup.blocksRaycasts = true;
    }

    public void CloseMainMenu()
    {
        m_canvasGroup.alpha = 0.0f;
        m_canvasGroup.interactable = false;
        m_canvasGroup.blocksRaycasts = false;
        PlayerPrefs.SetString("PlayerName", m_playerNameInputField.text);
        m_playerName = PlayerPrefs.GetString("PlayerName");
        m_playerNameInputField.text = m_playerName;
    }

    private void Awake()
    {
        m_startGameButton.onClick.AddListener(StartGame);
        m_joinSessionButton.onClick.AddListener(JoinSession);
        m_playerName = PlayerPrefs.GetString("PlayerName");
        m_playerNameInputField.text = m_playerName;
    }

    string m_customLobbyName = "TestLobby";

    private static System.Random random = new System.Random();
    public static string GenerateRandomString()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const int length = 4;
        StringBuilder result = new StringBuilder(length);

        for (int i = 0; i < length; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }

        return result.ToString();
    }

    private void StartGame()
    {
        GameManager.instance.LoadingVisual.gameObject.SetActive(true);
        m_mainMenuUI.alpha = 0.0f;
        m_mainMenuUI.blocksRaycasts = false;
        m_mainMenuUI.interactable = false;
        StartGameAsync();
    }

    async void StartGameAsync()
    {
        m_currentSessionName = GenerateRandomString();

        if(m_networkRunner != null)
        {
            Destroy(m_networkRunner.gameObject);
        }
        m_networkRunner = Instantiate(runnerPrefab);

        var result = await m_networkRunner.StartGame(new StartGameArgs
        {
            SessionName = m_currentSessionName,
            CustomLobbyName = m_customLobbyName,
            EnableClientSessionCreation = true,
            PlayerCount = 10,
            IsOpen = true,
            IsVisible = true,
            MatchmakingMode = MatchmakingMode.FillRoom,
            GameMode = GameMode.Shared
        });

        if (result.Ok)
        {
            Debug.Log("Game session started successfully!");
        }
        else
        {
            Debug.LogError($"Failed to start session: {result.ShutdownReason}");
        }
    }

    public void JoinSession()
    {
        Popup.Instance.Display(true, JoinActiveSession, Popup.Instance.Close, "Enter Session Code", "Join Session", "Close");
    }

    public void JoinActiveSession()
    {
        m_mainMenuUI.alpha = 0.0f;
        m_mainMenuUI.blocksRaycasts = false;
        m_mainMenuUI.interactable = false;
        GameManager.instance.LoadingVisual.gameObject.SetActive(true);
        m_currentSessionName = Popup.Instance.InputField.text.ToUpper();
        JoinActiveSessionAsync();
    }

    async void JoinActiveSessionAsync()
    {

        if (m_networkRunner != null)
        {
            Destroy(m_networkRunner.gameObject);
        }
        m_networkRunner = Instantiate(runnerPrefab);

        var result = await m_networkRunner.StartGame(new StartGameArgs
        {
            SessionName = m_currentSessionName,
            CustomLobbyName = m_customLobbyName,
            EnableClientSessionCreation = true,
            PlayerCount = 10,
            IsOpen = true,
            IsVisible = true,
            MatchmakingMode = MatchmakingMode.FillRoom,
            GameMode = GameMode.Shared
        });

        if (result.Ok)
        {
            Debug.Log("Game session started successfully!");
        }
        else
        {
            Debug.LogError($"Failed to start session: {result.ShutdownReason}");
        }

        Popup.Instance.Close();
    }
}
