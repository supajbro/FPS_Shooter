using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour, INetworkRunnerCallbacks
{

    public static GameManager instance;

    [Header("Network")]
    [SerializeField] private FusionBootstrap m_fusion;
    public FusionBootstrap Fusion => m_fusion;

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
        test.onClick.AddListener(StartGame);
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

    public Button test;

    void StartGame()
    {
        m_fusion.StartSharedClient();
    }

    public Transform sessionListContainer;
    public NetworkRunner networkRunner;

    //public async void RefreshSessionList()
    //{
    //    Debug.Log("Refreshing session list...");

    //    // Ensure we're in a lobby before fetching sessions
    //    var joinLobbyResult = await networkRunner.JoinSessionLobby(SessionLobby.Custom);

    //    if (!joinLobbyResult.Ok)
    //    {
    //        Debug.LogError($"Failed to join lobby: {joinLobbyResult.ErrorMessage}");
    //        return;
    //    }

    //    List<SessionInfo> sessions = networkRunner.LobbyInfo?.;

    //    // Clear existing buttons
    //    foreach (Transform child in sessionListContainer)
    //    {
    //        Destroy(child.gameObject);
    //    }

    //    if (sessions == null || sessions.Count == 0)
    //    {
    //        Debug.Log("No active sessions found.");
    //        return;
    //    }

    //    foreach (var session in sessions)
    //    {
    //        GameObject newButton = Instantiate(sessionButtonPrefab, sessionListContainer);
    //        newButton.GetComponentInChildren<Text>().text = $"{session.Name} ({session.PlayerCount}/{session.MaxPlayers})";
    //        newButton.GetComponent<Button>().onClick.AddListener(() => JoinGame(session.Name));
    //    }
    //}

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public List<SessionInfo> sessions = new();
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        foreach (SessionInfo session in sessionList)

        {

            // Display session information in your UI (e.g., session.Name, session.Region)

            Debug.Log($"Session Name: {session.Name}, Region: {session.Region}");

        }
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
}
