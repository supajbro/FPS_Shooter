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
    public FusionBootstrap Fusion => m_fusion;

    [SerializeField] private NetworkRunner m_networkRunner;

    [SerializeField] private Button m_startGameButton;
    [SerializeField] private Button m_joinSessionButton;
    [SerializeField] private CanvasGroup m_mainMenuUI;

    [Header("UI")]
    public CanvasGroup m_canvasGroup;

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
    }

    private void Awake()
    {
        m_startGameButton.onClick.AddListener(StartGame);
        m_joinSessionButton.onClick.AddListener(JoinSession);
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
        StartGameAsync();
    }

    async void StartGameAsync()
    {
        var result = await m_networkRunner.StartGame(new StartGameArgs
        {
            SessionName = GenerateRandomString(),
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
        GameManager.instance.LoadingVisual.gameObject.SetActive(true);
        JoinActiveSessionAsync();
    }

    async void JoinActiveSessionAsync()
    {
        var result = await m_networkRunner.StartGame(new StartGameArgs
        {
            SessionName = Popup.Instance.InputField.text,
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
